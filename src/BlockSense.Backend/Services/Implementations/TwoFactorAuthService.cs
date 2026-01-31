using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Exceptions.TwoFactorAuthentication;
using BlockSense.Backend.Exceptions.User;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Cryptography.Encryption;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.Cryptography.Utilities;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using Microsoft.Extensions.Options;
using OtpNet;
using QRCoder;
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
            IOptions<TwoFactorAuthConfig> twoFactorAuthConfig,
            ITwoFactorAuthRepository twoFactorAuthRepository,
            IUserRepository userRepository)
        {
            _twoFactorAuthConfig = twoFactorAuthConfig.Value
                ?? throw new ArgumentNullException(nameof(twoFactorAuthConfig));

            _twoFactorAuthRepository = twoFactorAuthRepository
                ?? throw new ArgumentNullException(nameof(twoFactorAuthRepository));

            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<TwoFactorSetupInit> SetupInitAsync(uint userId, CancellationToken cancellationToken = default)
        {
            if (await _twoFactorAuthRepository.IsEnabledAsync(userId, cancellationToken))
            {
                throw new TwoFactorAlreadyConfiguredException();
            }

            var user =
                await _userRepository.GetByIdAsync(userId, cancellationToken)
                ?? throw new UserNotFoundException();

            string setupKey = Base32Encoding.ToString(
                CryptographyUtilities.GenerateSecureRandomBytes(20));

            string authUri =
                GenerateAuthUri(user.Email, setupKey);

            var qrCodeData =
                GenerateQRCodeData(authUri);

            return new TwoFactorSetupInit
            {
                SetupKey = setupKey,
                QRCodeData = qrCodeData
            };
        }

        public async Task CompleteSetupAsync(uint userId, TwoFactorSetupRequest request, CancellationToken cancellationToken = default)
        {
            if (await _twoFactorAuthRepository.IsEnabledAsync(userId, cancellationToken))
            {
                throw new TwoFactorAlreadyConfiguredException();
            }

            request = request with
            {
                SecretKey = request.SecretKey.Trim(),
                TwoFactorCode = request.TwoFactorCode.Trim().ToUpperInvariant()
            };

            byte[] secretKey = Base32Encoding.ToBytes(request.SecretKey);

            if (!VerifyCode(secretKey, request.TwoFactorCode))
            {
                throw new TwoFactorInvalidCodeException();
            }

            var aes256GcmEncryptor = new Aes256GcmEncryptor();

            byte[] key =
                Convert.FromBase64String(_twoFactorAuthConfig.MasterKey);

            byte[] iv =
                CryptographyUtilities.GenerateSecureRandomBytes(12);

            byte[] ciphertext =
                aes256GcmEncryptor.Encrypt(key, iv, secretKey);

            // Combine nonce (12) + cipherTextWithTag (36) = 48 bytes
            var encryptedSecret = iv.Concat(ciphertext).ToArray();

            var twoFaAuthEntity = new TwoFactorAuthEntity
            {
                UserId = userId,
                EncryptedTotpSecret = encryptedSecret,
                UpdatedAt = DateTime.UtcNow
            };

            await _twoFactorAuthRepository.CreateAsync(twoFaAuthEntity, cancellationToken);
        }

        public async Task VerifyAsync(uint userId, TwoFactorVerificationRequest request, CancellationToken cancellationToken = default)
        {
            var code = request.TwoFactorCode.Trim().ToUpperInvariant();

            var twoFaAuthEntity =
                await _twoFactorAuthRepository.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new TwoFactorNotConfiguredException();

            if (twoFaAuthEntity.BackupCodes is not null &&
                VerifyBackupCode(twoFaAuthEntity.BackupCodes, code))
            {
                twoFaAuthEntity.RemoveBackupCode(code);

                await _twoFactorAuthRepository.UpdateBackupCodesAsync(userId, twoFaAuthEntity.BackupCodes, cancellationToken);
                return;
            }

            byte[] key =
                Convert.FromBase64String(_twoFactorAuthConfig.MasterKey);

            byte[] iv =
                twoFaAuthEntity.EncryptedTotpSecret.Take(12).ToArray();

            byte[] cipherText =
                twoFaAuthEntity.EncryptedTotpSecret.Skip(12).ToArray();

            var aes256GcmEncryptor = new Aes256GcmEncryptor();

            byte[] decryptedSecret = aes256GcmEncryptor.Decrypt(key, iv, cipherText);

            if (VerifyCode(decryptedSecret, code))
            {
                return;
            }

            throw new TwoFactorInvalidCodeException();
        }

        public async Task<IReadOnlyList<string>> GenerateBackupCodesAsync(uint userId, CancellationToken cancellationToken = default)
        {
            var twoFaAuthEntity =
                await _twoFactorAuthRepository.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new TwoFactorNotConfiguredException();

            var now = DateTime.UtcNow;
            var cooldown = _twoFactorAuthConfig.BackupCodeCooldown;

            if (twoFaAuthEntity.BackupCodes is not null)
            {
                var elapsed = now - twoFaAuthEntity.UpdatedAt;

                if (elapsed < cooldown)
                    throw new TwoFactorCooldownException(cooldown - elapsed);
            }

            var backupCodes = GenerateBackupCodes();

            var hashedBackupCodes = backupCodes
                .Select(code => Sha256Hasher.ComputeBase64(
                    Encoding.UTF8.GetBytes(code)))
                .ToList();

            await _twoFactorAuthRepository.InsertBackupCodesAsync(
                userId,
                hashedBackupCodes,
                now,
                cancellationToken);

            return backupCodes;
        }

        public async Task DisableAsync(uint userId, TwoFactorVerificationRequest request, CancellationToken cancellationToken = default)
        {
            if (!await _twoFactorAuthRepository.IsEnabledAsync(userId, cancellationToken))
            {
                throw new TwoFactorNotConfiguredException();
            }

            await VerifyAsync(userId, request);

            await _twoFactorAuthRepository.DisableAsync(userId, cancellationToken);
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
                return false;

            var backupCodeHash = Sha256Hasher.ComputeByte(Encoding.UTF8.GetBytes(code));

            foreach (var backupCode in backupCodes)
            {
                if (CryptographicOperations.FixedTimeEquals(backupCodeHash, Convert.FromBase64String(backupCode)) == false)
                    continue;

                return true;
            }

            return false;
        }

        private IReadOnlyList<string> GenerateBackupCodes()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            return Enumerable
                .Range(0, _twoFactorAuthConfig.BackupCodeCount)
                .Select(_ =>
                {
                    var bytes = CryptographyUtilities
                        .GenerateSecureRandomBytes(_twoFactorAuthConfig.BackupCodeLength);

                    var code = string.Concat(
                        bytes.Select((b, i) =>
                            (i == 4 ? "-" : "") + chars[b % chars.Length]));

                    return code;
                })
                .ToList();
        }

        private string GenerateAuthUri(string userEmail, string secretKey)
        {
            return $"otpauth://totp/{Uri.EscapeDataString(_twoFactorAuthConfig.Issuer)}:{Uri.EscapeDataString(userEmail)}?" +
                   $"secret={secretKey}&issuer={Uri.EscapeDataString(_twoFactorAuthConfig.Issuer)}" +
                   $"&algorithm=SHA1&digits={_twoFactorAuthConfig.CodeLength}&period={_twoFactorAuthConfig.CodeLifetime.TotalSeconds}";
        }

        private byte[] GenerateQRCodeData(string authUri)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(authUri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }
    }
}
