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
using System.Security.Cryptography;
using System.Text;

namespace BlockSense.Backend.Services.Implementations
{
    /// <summary>
    /// Provides two-factor authentication (TOTP) management, including setup, verification, backup codes, and disabling.
    /// </summary>
    public sealed class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly TwoFactorAuthConfig _twoFactorAuthConfig;
        private readonly ITotpCredentialRepository _totpCredentialRepository;
        private readonly IUserRepository _userRepository;
        private readonly Aes256GcmEncryptor _aes256GcmEncryptor;

        /// <summary>
        /// Initializes a new instance of <see cref="TwoFactorAuthService"/> with required dependencies.
        /// </summary>
        /// <param name="twoFactorAuthConfig">The configuration for TOTP settings, backup codes, and encryption.</param>
        /// <param name="totpCredentialRepository">The repository for TOTP credential entity operations.</param>
        /// <param name="userRepository">The repository for user entity operations.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is <c>null</c>.</exception>
        public TwoFactorAuthService(
            IOptions<TwoFactorAuthConfig> twoFactorAuthConfig,
            ITotpCredentialRepository totpCredentialRepository,
            IUserRepository userRepository)
        {
            _twoFactorAuthConfig = twoFactorAuthConfig.Value ?? throw new ArgumentNullException(nameof(twoFactorAuthConfig));
            _totpCredentialRepository = totpCredentialRepository ?? throw new ArgumentNullException(nameof(totpCredentialRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _aes256GcmEncryptor = new Aes256GcmEncryptor();
        }

        /// <inheritdoc/>
        public async Task<TwoFactorSetupInit> SetupInitAsync(uint userId, CancellationToken cancellationToken = default)
        {
            await EnsureTwoFactorNotConfiguredAsync(userId, cancellationToken);

            var user = await GetUserOrThrowAsync(userId, cancellationToken);

            var setupKey = Base32Encoding.ToString(CryptographyUtilities.GenerateSecureRandomBytes(20));
            var authUri = BuildAuthUri(user.Email, setupKey);
            var qrCodeData = GenerateQrCodePng(authUri);

            return new TwoFactorSetupInit
            {
                SetupKey = setupKey,
                QrCodeData = qrCodeData
            };
        }

        /// <inheritdoc/>
        public async Task CompleteSetupAsync(
            uint userId,
            TwoFactorSetupRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureTwoFactorNotConfiguredAsync(userId, cancellationToken);

            var normalizedRequest = NormalizeSetupRequest(request);
            var secretKey = Base32Encoding.ToBytes(normalizedRequest.SetupKey);

            if (!VerifyTotpCode(secretKey, normalizedRequest.TwoFactorCode))
                throw new TwoFactorInvalidCodeException();

            var credential = BuildTotpCredential(userId, secretKey);
            await _totpCredentialRepository.CreateAsync(credential, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task VerifyAsync(uint userId, TwoFactorVerificationRequest request, CancellationToken cancellationToken = default)
        {
            var normalizedCode = NormalizeCode(request.TwoFactorCode);

            var credential = await _totpCredentialRepository.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new TwoFactorConfigurationException();

            var decryptedSecret = DecryptSecret(credential.EncryptedSecret);

            if (await TryVerifyAndConsumeBackupCodeAsync(credential, normalizedCode, cancellationToken))
                return;

            if (VerifyTotpCode(decryptedSecret, normalizedCode))
                return;

            throw new TwoFactorInvalidCodeException();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<string>> GenerateBackupCodesAsync(uint userId, CancellationToken cancellationToken = default)
        {
            var credential = await _totpCredentialRepository.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new TwoFactorConfigurationException();

            EnforceBackupCodeCooldown(credential);

            var plainCodes = GenerateBackupCodes();
            var hashedCodes = HashBackupCodes(plainCodes);

            credential.BackupCodesList = hashedCodes;
            credential.UpdatedAt = DateTime.UtcNow;

            await _totpCredentialRepository.UpdateAsync(credential, cancellationToken);

            return plainCodes;
        }

        /// <inheritdoc/>
        public async Task DisableAsync(uint userId, TwoFactorVerificationRequest request, CancellationToken cancellationToken = default)
        {
            if (!await _totpCredentialRepository.ExistsAsync(userId, cancellationToken))
                throw new TwoFactorConfigurationException();

            await VerifyAsync(userId, request, cancellationToken);

            await _totpCredentialRepository.DeleteByUserIdAsync(userId, cancellationToken);
        }

        private async Task EnsureTwoFactorNotConfiguredAsync(uint userId, CancellationToken cancellationToken)
        {
            if (await _totpCredentialRepository.ExistsAsync(userId, cancellationToken))
                throw new TwoFactorConfigurationException();
        }

        private async Task<Entities.User> GetUserOrThrowAsync(uint userId, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user is null)
                throw new NotFoundException();

            return user;
        }

        private static TwoFactorSetupRequest NormalizeSetupRequest(TwoFactorSetupRequest request) =>
            request with
            {
                SetupKey = request.SetupKey.Trim(),
                TwoFactorCode = NormalizeCode(request.TwoFactorCode)
            };

        private static string NormalizeCode(string code) =>
            code.Trim().ToUpperInvariant();

        private TotpCredential BuildTotpCredential(uint userId, byte[] secretKey)
        {
            var encryptedSecret = EncryptSecret(secretKey);
            var now = DateTime.UtcNow;

            return new TotpCredential
            {
                UserId = userId,
                EncryptedSecret = encryptedSecret,
                BackupCodes = null,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        private bool VerifyTotpCode(byte[] secretKey, string code)
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

        private async Task<bool> TryVerifyAndConsumeBackupCodeAsync(
            TotpCredential credential,
            string code,
            CancellationToken cancellationToken)
        {
            if (!credential.HasBackupCodes)
                return false;

            var codeHash = Sha256Hasher.ComputeBytes(Encoding.UTF8.GetBytes(code));

            var matchedCode = credential.BackupCodesList
                .FirstOrDefault(b => CryptographicOperations.FixedTimeEquals(
                    codeHash,
                    Convert.FromBase64String(b)));

            if (matchedCode is null)
                return false;

            credential.BackupCodesList.Remove(matchedCode);
            await _totpCredentialRepository.UpdateAsync(credential, cancellationToken);

            return true;
        }

        private IReadOnlyList<string> GenerateBackupCodes()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            return Enumerable
                .Range(0, _twoFactorAuthConfig.BackupCodeCount)
                .Select(_ => BuildBackupCode(chars))
                .ToList()
                .AsReadOnly();
        }

        private string BuildBackupCode(string chars)
        {
            var bytes = CryptographyUtilities.GenerateSecureRandomBytes(_twoFactorAuthConfig.BackupCodeLength);

            return string.Concat(
                bytes.Select((b, i) => (i == 4 ? "-" : "") + chars[b % chars.Length]));
        }

        private void EnforceBackupCodeCooldown(TotpCredential credential)
        {
            if (!credential.HasBackupCodes)
                return;

            var elapsed = DateTime.UtcNow - credential.UpdatedAt;
            var cooldown = _twoFactorAuthConfig.BackupCodeCooldown;

            if (elapsed < cooldown)
                throw new TwoFactorCooldownException(cooldown - elapsed);
        }

        private static List<string> HashBackupCodes(IReadOnlyList<string> codes) =>
            codes.Select(code => Sha256Hasher.ComputeBase64(Encoding.UTF8.GetBytes(code)))
                 .ToList();

        private string BuildAuthUri(string userEmail, string secretKey) =>
            $"otpauth://totp/{Uri.EscapeDataString(_twoFactorAuthConfig.Issuer)}:{Uri.EscapeDataString(userEmail)}?" +
            $"secret={secretKey}" +
            $"&issuer={Uri.EscapeDataString(_twoFactorAuthConfig.Issuer)}" +
            $"&algorithm=SHA1" +
            $"&digits={_twoFactorAuthConfig.CodeLength}" +
            $"&period={_twoFactorAuthConfig.CodeLifetime.TotalSeconds}";

        private static byte[] GenerateQrCodePng(string authUri)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(authUri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }

        private byte[] EncryptSecret(byte[] secret)
        {
            var key = Convert.FromBase64String(_twoFactorAuthConfig.MasterKey);
            var iv = CryptographyUtilities.GenerateSecureRandomBytes(12);
            var ciphertext = _aes256GcmEncryptor.Encrypt(key, iv, secret);
            return iv.Concat(ciphertext).ToArray();
        }

        private byte[] DecryptSecret(byte[] encryptedSecret)
        {
            var key = Convert.FromBase64String(_twoFactorAuthConfig.MasterKey);
            var iv = encryptedSecret.Take(12).ToArray();
            var ciphertext = encryptedSecret.Skip(12).ToArray();
            return _aes256GcmEncryptor.Decrypt(key, iv, ciphertext);
        }
    }
}