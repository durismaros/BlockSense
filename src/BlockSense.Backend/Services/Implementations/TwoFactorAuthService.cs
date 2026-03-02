using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Exceptions.Generic;
using BlockSense.Backend.Exceptions.TwoFactorAuthentication;
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
using System.Net;
using System.Security.Cryptography;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace BlockSense.Backend.Services.Implementations
{
    public sealed class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly TwoFactorAuthConfig _twoFactorAuthConfig;
        private readonly ITotpCredentialRepository _twoFactorAuthRepository;
        private readonly IUserRepository _userRepository;
        private readonly Aes256GcmEncryptor _aes256GcmEncryptor;

        public TwoFactorAuthService(
            IOptions<TwoFactorAuthConfig> twoFactorAuthConfig,
            ITotpCredentialRepository twoFactorAuthRepository,
            IUserRepository userRepository)
        {
            _twoFactorAuthConfig = twoFactorAuthConfig.Value ?? throw new ArgumentNullException(nameof(twoFactorAuthConfig));
            _twoFactorAuthRepository = twoFactorAuthRepository ?? throw new ArgumentNullException(nameof(twoFactorAuthRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _aes256GcmEncryptor = new Aes256GcmEncryptor() ?? throw new ArgumentNullException(nameof(Aes256GcmEncryptor));
        }

        public async Task<TwoFactorSetupInit> SetupInitAsync(uint userId, CancellationToken cancellationToken = default)
        {
            if (await _twoFactorAuthRepository.ExistsAsync(userId, cancellationToken))
            {
                throw new TwoFactorConfigurationException();
            }

            var user =
                await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                throw new NotFoundException();
            }

            string setupKey = Base32Encoding.ToString(
                CryptographyUtilities.GenerateSecureRandomBytes(20));

            var authUri = GenerateAuthUri(user.Email, setupKey);
            var qrCodeData = GenerateQRCodeData(authUri);

            return new TwoFactorSetupInit
            {
                SetupKey = setupKey,
                QRCodeData = qrCodeData
            };
        }

        public async Task CompleteSetupAsync(
            uint userId,
            TwoFactorSetupRequest request,
            CancellationToken cancellationToken = default)
        {
            if (await _twoFactorAuthRepository.ExistsAsync(userId, cancellationToken))
            {
                throw new TwoFactorConfigurationException();
            }

            request = request with
            {
                SetupKey = request.SetupKey.Trim(),
                TwoFactorCode = request.TwoFactorCode.Trim().ToUpperInvariant()
            };

            byte[] secretKey = Base32Encoding.ToBytes(request.SetupKey);

            if (!VerifyCode(secretKey, request.TwoFactorCode))
            {
                throw new TwoFactorInvalidCodeException();
            }

            var encryptedSecret = EncryptSecret(secretKey);
            var now = DateTime.UtcNow;

            var twoFaAuthEntity = new TotpCredential
            {
                UserId = userId,
                EncryptedSecret = encryptedSecret,
                BackupCodes = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _twoFactorAuthRepository.CreateAsync(twoFaAuthEntity, cancellationToken);
        }

        public async Task VerifyAsync(uint userId, TwoFactorVerificationRequest request, CancellationToken cancellationToken = default)
        {
            var code = request.TwoFactorCode.Trim().ToUpperInvariant();

            var totpCredential =
                await _twoFactorAuthRepository.GetByUserIdAsync(userId, cancellationToken);

            if (totpCredential is null)
            {
                throw new TwoFactorConfigurationException();
            }

            var decryptedSecret =
                DecryptSecret(totpCredential.EncryptedSecret);

            if (await VerifyAndConsumeBackupCodeAsync(totpCredential, code, cancellationToken))
            {
                return;
            }

            if (VerifyCode(decryptedSecret, code))
            {
                return;
            }

            throw new TwoFactorInvalidCodeException();
        }

        public async Task<IReadOnlyList<string>> GenerateBackupCodesAsync(uint userId, CancellationToken cancellationToken = default)
        {
            var totpCredential =
                await _twoFactorAuthRepository.GetByUserIdAsync(userId, cancellationToken);

            if (totpCredential is null)
            {
                throw new TwoFactorConfigurationException();
            }

            EnforceBackupCodeCooldown(totpCredential);

            var plainCodes = GenerateBackupCodes();
            var hashedBackupCodes = HashBackupCodes(plainCodes);

            totpCredential.BackupCodesList = hashedBackupCodes;
            totpCredential.UpdatedAt = DateTime.UtcNow;

            await _twoFactorAuthRepository.UpdateAsync(totpCredential, cancellationToken);

            return plainCodes;
        }

        public async Task DisableAsync(uint userId, TwoFactorVerificationRequest request, CancellationToken cancellationToken = default)
        {
            if (!await _twoFactorAuthRepository.ExistsAsync(userId, cancellationToken))
            {
                throw new TwoFactorConfigurationException();
            }

            await VerifyAsync(userId, request);

            await _twoFactorAuthRepository.DeleteByUserIdAsync(userId, cancellationToken);
        }

        private bool VerifyCode(byte[] secretKey, string code)
        {
            try
            {
                var totp = new Totp(secretKey);

                return totp.VerifyTotp(
                    code,
                    out _,
                    VerificationWindow.RfcSpecifiedNetworkDelay);
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> VerifyAndConsumeBackupCodeAsync(TotpCredential totpCredential, string code, CancellationToken cancellationToken)
        {
            if (!totpCredential.HasBackupCodes)
            {
                return false;
            }

            var codeHash = Sha256Hasher.ComputeByte(
                Encoding.UTF8.GetBytes(code));

            var matchedCode = totpCredential.BackupCodesList
                .FirstOrDefault(b =>
                    CryptographicOperations.FixedTimeEquals(
                        codeHash,
                        Convert.FromBase64String(b)));

            if (matchedCode is null)
            {
                return false;
            }

            totpCredential.BackupCodesList.Remove(matchedCode);

            await _twoFactorAuthRepository.UpdateAsync(totpCredential, cancellationToken);
            return true;
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
                .ToList()
                .AsReadOnly();
        }

        private void EnforceBackupCodeCooldown(TotpCredential totpCredential)
        {
            if (!totpCredential.HasBackupCodes)
            {
                return;
            }

            var cooldown = _twoFactorAuthConfig.BackupCodeCooldown;

            var elapsed =
                DateTime.UtcNow - totpCredential.UpdatedAt;

            if (elapsed < cooldown)
            {
                throw new TwoFactorCooldownException(cooldown - elapsed);
            }
        }

        private static List<string> HashBackupCodes(IReadOnlyList<string> codes)
            => codes.Select(code => Sha256Hasher.ComputeBase64(Encoding.UTF8.GetBytes(code)))
                    .ToList();

        private string GenerateAuthUri(string userEmail, string secretKey)
        {
            return $"otpauth://totp/{Uri.EscapeDataString(_twoFactorAuthConfig.Issuer)}:{Uri.EscapeDataString(userEmail)}?" +
                   $"secret={secretKey}" +
                   $"&issuer={Uri.EscapeDataString(_twoFactorAuthConfig.Issuer)}" +
                   $"&algorithm=SHA1" +
                   $"&digits={_twoFactorAuthConfig.CodeLength}" +
                   $"&period={_twoFactorAuthConfig.CodeLifetime.TotalSeconds}";
        }

        private byte[] GenerateQRCodeData(string authUri)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(authUri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }

        private byte[] EncryptSecret(byte[] secret)
        {
            var key =
                Convert.FromBase64String(_twoFactorAuthConfig.MasterKey);

            var iv =
                CryptographyUtilities.GenerateSecureRandomBytes(12);

            var ciphertext = _aes256GcmEncryptor.Encrypt(key, iv, secret);
            return iv.Concat(ciphertext).ToArray();
        }

        private byte[] DecryptSecret(byte[] encryptedSecret)
        {
            var key =
                Convert.FromBase64String(_twoFactorAuthConfig.MasterKey);

            var iv =
                encryptedSecret.Take(12).ToArray();

            var ciphertext = encryptedSecret.Skip(12).ToArray();
            return _aes256GcmEncryptor.Decrypt(key, iv, ciphertext);
        }
    }
}
