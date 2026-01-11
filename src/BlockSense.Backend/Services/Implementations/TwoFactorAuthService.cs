using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Cryptography.Encryption;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.Cryptography.Utilities;
using BlockSense.Contracts.DTOs.TwoFactorAuth;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using OtpNet;
using QRCoder;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace BlockSense.Backend.Services.Implementations
{
    public sealed class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly TwoFactorAuthConfig _twoFactorAuthConfig;
        private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
        private readonly IUserRepository _userRepository;

        public TwoFactorAuthService(
            TwoFactorAuthConfig twoFactorAuthConfig,
            ITwoFactorAuthRepository twoFactorAuthRepository,
            IUserRepository userRepository)
        {
            _twoFactorAuthConfig = twoFactorAuthConfig ?? throw new ArgumentNullException(nameof(twoFactorAuthConfig));
            _twoFactorAuthRepository = twoFactorAuthRepository ?? throw new ArgumentNullException(nameof(twoFactorAuthRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<bool> VerifyAsync(uint userId, string code)
        {
            code = code.Trim().ToUpperInvariant();

            var twoFaAuthEntity = await _twoFactorAuthRepository.GetByUserIdAsync(userId);

            if (twoFaAuthEntity is null || twoFaAuthEntity.EncryptedTotpSecret is null)
            {
                throw new InvalidOperationException("Two-factor authentication is not enabled for this user.");
            }

            if (twoFaAuthEntity.BackupCodes is not null &&
                VerifyBackupCode(twoFaAuthEntity.BackupCodes, code))
            {
                twoFaAuthEntity.RemoveBackupCode(code);
                twoFaAuthEntity.UpdatedAt = DateTime.UtcNow;

                await _twoFactorAuthRepository.CreateOrUpdateAsync(twoFaAuthEntity);
                return true;
            }

            byte[] iv = new byte[12];
            Buffer.BlockCopy(twoFaAuthEntity.EncryptedTotpSecret, 0, iv, 0, 12);

            byte[] cipherText = new byte[twoFaAuthEntity.EncryptedTotpSecret.Length - iv.Length];
            Buffer.BlockCopy(twoFaAuthEntity.EncryptedTotpSecret, 12, cipherText, 0, cipherText.Length);

            var aes256GcmEncryptor = new Aes256GcmEncryptor();

            byte[] key = Convert.FromBase64String(_twoFactorAuthConfig.MasterKey);
            byte[] decryptedSecret = aes256GcmEncryptor.Decrypt(key, iv, cipherText);

            return VerifyCode(decryptedSecret, code);
        }

        public async Task<TwoFactorSetupInit> SetupInitAsync(uint userId)
        {
            if (await _twoFactorAuthRepository.IsEnabledAsync(userId))
            {
                throw new InvalidOperationException("Two-factor authentication is already enabled.");
            }

            var user = await _userRepository.GetByIdAsync(userId);

            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException("Two-factor authentication cannot be enabled for this user.");
            }

            string setupKey = Base32Encoding.ToString(
                CryptographyUtilities.GenerateSecureRandomBytes(20));

            string authUri = GenerateAuthUri(user.Email, setupKey);
            var qrCodeData = GenerateQRCodeData(authUri);

            return new TwoFactorSetupInit
            {
                SetupKey = setupKey,
                QRCodeData = qrCodeData
            };
        }

        public async Task<bool> CompleteSetupAsync(uint userId, TwoFactorSetupRequest request)
        {
            if (await _twoFactorAuthRepository.IsEnabledAsync(userId))
            {
                throw new InvalidOperationException("Two-factor authentication is already enabled.");
            }

            request = request with
            {
                SecretKey = request.SecretKey.Trim(),
                TwoFactorCode = request.TwoFactorCode.Trim().ToUpperInvariant()
            };

            byte[] secretKey = Base32Encoding.ToBytes(request.SecretKey);

            if (!VerifyCode(secretKey, request.TwoFactorCode))
            {
                return false;
            }

            var aes256GcmEncryptor = new Aes256GcmEncryptor();

            byte[] key = Convert.FromBase64String(_twoFactorAuthConfig.MasterKey);
            byte[] iv = CryptographyUtilities.GenerateSecureRandomBytes(12);

            byte[] ciphertext = aes256GcmEncryptor.Encrypt(key, iv, secretKey);

            // Combine nonce (12) + cipherTextWithTag (36) = 48 bytes
            var encryptedSecret = new byte[iv.Length + ciphertext.Length];
            Buffer.BlockCopy(iv, 0, encryptedSecret, 0, iv.Length);
            Buffer.BlockCopy(ciphertext, 0, encryptedSecret, iv.Length, ciphertext.Length);

            var twoFaAuthEntity = new TwoFactorAuthEntity
            {
                UserId = userId,
                EncryptedTotpSecret = encryptedSecret,
                UpdatedAt = DateTime.UtcNow
            };

            await _twoFactorAuthRepository.CreateOrUpdateAsync(twoFaAuthEntity);
            return true;
        }

        public async Task<bool> DisableAsync(uint userId, string code)
        {
            if (!await VerifyAsync(userId, code))
            {
                throw new InvalidOperationException("not verified");
            }

            await _twoFactorAuthRepository.DisableAsync(userId);
            return true;
        }

        public async Task<TwoFactorBackupResponse> GenerateBackupAsync(uint userId)
        {
            var twoFaAuthEntity = await _twoFactorAuthRepository.GetByUserIdAsync(userId);

            if (twoFaAuthEntity is null || twoFaAuthEntity.EncryptedTotpSecret is null)
            {
                throw new InvalidOperationException("Two-factor authentication is not enabled for this user.");
            }
            
            var backupCodes = GenerateBackupCodes();

            twoFaAuthEntity.UpdatedAt = DateTime.UtcNow;
            twoFaAuthEntity.BackupCodes = backupCodes
                .Select(code => Sha256Hasher.ComputeBase64(Encoding.UTF8.GetBytes(code)))
                .ToList();

            await _twoFactorAuthRepository.CreateOrUpdateAsync(twoFaAuthEntity);

            return new TwoFactorBackupResponse
            {
                Status 
                Codes = backupCodes
            };
        }

        private bool VerifyCode(byte[] secretKey, string code)
        {
            try
            {
                var totp = new Totp(secretKey);
                return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
            }
            catch
            {
                return false;
            }
        }

        private bool VerifyBackupCode(IEnumerable<string> backupCodes, string code)
        {
            if (backupCodes is null)
            {
                return false;
            }

            var backupCodeHash = Sha256Hasher.ComputeByte(Encoding.UTF8.GetBytes(code));

            foreach (var backupCode in backupCodes)
            {
                if (!CryptographicOperations.FixedTimeEquals(backupCodeHash, Convert.FromBase64String(backupCode)))
                    continue;

                return true;
            }

            return false;
        }

        private string GenerateAuthUri(string userEmail, string secretKey)
        {
            return $"otpauth://totp/{Uri.EscapeDataString(_twoFactorAuthConfig.Issuer)}:{Uri.EscapeDataString(userEmail)}?" +
                   $"secret={secretKey}&issuer={Uri.EscapeDataString(_twoFactorAuthConfig.Issuer)}" +
                   $"&algorithm=SHA1&digits={_twoFactorAuthConfig.CodeLength}&period={_twoFactorAuthConfig.CodeLifetime}";
        }

        private byte[] GenerateQRCodeData(string authUri)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(authUri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }

        private IReadOnlyList<string> GenerateBackupCodes()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            var backupCodes = new List<string>();

            for (int i = 0; i < _twoFactorAuthConfig.BackupCodeCount; i++)
            {
                byte[] randomBytes = CryptographyUtilities.GenerateSecureRandomBytes(_twoFactorAuthConfig.BackupCodeLength);
                var code = new StringBuilder(_twoFactorAuthConfig.BackupCodeLength);

                for (int j = 0; j < _twoFactorAuthConfig.BackupCodeLength - 1; j++)
                {
                    if (j == 4)
                        code.Append('-');

                    code.Append(chars[randomBytes[j] % chars.Length]);
                }

                backupCodes.Add(code.ToString());
            }

            return backupCodes;
        }
    }
}
