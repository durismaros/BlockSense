using BlockSenseAPI.Cryptography;
using BlockSenseAPI.Cryptography.Encryption;
using BlockSenseAPI.Cryptography.Hashing;
using BlockSenseAPI.Models.TwoFactorAuth;
using BlockSenseAPI.Models.TwoFactorAuth.BackupCode;
using BlockSenseAPI.Models.TwoFactorAuth.Setup;
using BlockSenseAPI.Models.TwoFactorAuth.Verification;
using BlockSenseAPI.Services.TwoFactorAuth;
using Microsoft.Extensions.Options;
using OtpNet;
using QRCoder;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BlockSenseAPI.Services.UserServices
{
    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly TwoFactorConfig _config;
        private readonly DatabaseContext _dbContext;

        private const int SecretKeyLength = 20; // 160-bit secret for TOTP

        private const int BackupCodeCount = 5;
        private const int BackupCodeLength = 8;

        public TwoFactorAuthService(IOptions<TwoFactorConfig> twoFactorConfig, DatabaseContext dbContext)
        {
            _config = twoFactorConfig.Value;
            _dbContext = dbContext;
        }

        public async Task<TwoFactorSetupResponse?> BeginSetup(int userId)
        {
            string query = "select email from users where user_id = @user_id";

            Dictionary<string, object> parameters = new()
            {
                { "@user_id", userId },
            };

            using var reader = await _dbContext.ExecuteReaderAsync(query, parameters);

            if (!await reader.ReadAsync())
                return null;

            string email = reader.GetString("email");
            string secretKey = GenerateRandomSecretKey();
            string otpAuthUri = GenerateOtpAuthUri(email, secretKey, _config.Issuer);
            byte[] qrCodeData = GenerateQRCodeData(otpAuthUri);

            return new TwoFactorSetupResponse
            {
                SetupKey = secretKey,
                QRCodeData = qrCodeData
            };
        }

        public async Task<TwoFactorVerificationResponse?> CompleteSetup(int userId, TwoFactorSetupRequest request)
        {
            if (request is null || string.IsNullOrEmpty(request.SecretKey) || string.IsNullOrEmpty(request.Code) || request.Code.Length != 6)
                return null;

            if (!VerifyCode(Base32Encoding.ToBytes(request.SecretKey), request.Code))
                return new TwoFactorVerificationResponse
                {
                    Verification = false,
                    Message = "Invalid code"
                };

            var aes256GcmEncryptor = new Aes256GcmEncryptor();
            byte[] plaintext = Base32Encoding.ToBytes(request.SecretKey);
            byte[] key = Convert.FromBase64String(_config.MasterKey);
            byte[] iv = CryptographyUtilities.GenerateSecureRandomBytes(12);

            byte[] ciphertext = aes256GcmEncryptor.Encrypt(key, iv, plaintext);

            // Combine nonce (12) + cipherTextWithTag (36) = 48 bytes
            byte[] encryptedSecret = new byte[iv.Length + ciphertext.Length];
            Buffer.BlockCopy(iv, 0, encryptedSecret, 0, iv.Length);
            Buffer.BlockCopy(ciphertext, 0, encryptedSecret, iv.Length, ciphertext.Length);

            string query = "insert into two_factor_auth values(@user_id, true, @encrypted_totp_secret, null, default) " +
                "on duplicate key update is_2fa_enabled = if(is_2fa_enabled = false, true, false), encrypted_totp_secret = @encrypted_totp_secret";

            Dictionary<string, object> parameters = new()
            {
                { "@user_id",  userId},
                { "@encrypted_totp_secret", encryptedSecret }
            };

            if (await _dbContext.ExecuteNonQueryAsync(query, parameters) < 1)
                return new TwoFactorVerificationResponse
                {
                    Verification = false,
                    Message = "2fa could not be enabled"
                };

            return new TwoFactorVerificationResponse
            {
                Verification = true,
                Message = "2fa enabled successfully"
            };
        }

        public async Task<TwoFactorVerificationResponse?> VerifyOtp(int userId, string? code)
        {
            if (string.IsNullOrEmpty(code) || (code.Length != 6 && code.Length != 8))
                return null;

            code = code.ToUpper();

            if (await VerifyBackupCode(userId, Encoding.UTF8.GetBytes(code.ToUpper())))
                return new TwoFactorVerificationResponse
                {
                    Verification = true,
                    Message = "Verified using backup code"
                };

            string query = "select encrypted_totp_secret from two_factor_auth where user_id = @user_id and is_2fa_enabled = true";

            Dictionary<string, object> parameters = new()
            {
                {"@user_id", userId}
            };

            using var reader = await _dbContext.ExecuteReaderAsync(query, parameters);

            if (!await reader.ReadAsync())
                return new TwoFactorVerificationResponse
                {
                    Verification = false,
                    Message = "2fa not enabled for this user"
                };

            byte[] storedData = new byte[48];
            reader.GetBytes("encrypted_totp_secret", 0, storedData, 0, 48);

            // Extract nonce (12) + ciphertextWithTag (36)
            byte[] iv = new byte[12];
            byte[] ciphertext = new byte[storedData.Length - 12];

            Buffer.BlockCopy(storedData, 0, iv, 0, 12);
            Buffer.BlockCopy(storedData, 12, ciphertext, 0, ciphertext.Length);

            var aes256GcmEncryptor = new Aes256GcmEncryptor();

            byte[] key = Convert.FromBase64String(_config.MasterKey);
            byte[] decryptedSecret = aes256GcmEncryptor.Decrypt(key, iv, ciphertext);

            bool isValid = VerifyCode(decryptedSecret, code);

            return new TwoFactorVerificationResponse
            {
                Verification = isValid,
                Message = isValid ? "OTP verification successful" : "OTP verification failed"
            };
        }

        public async Task<TwoFactorVerificationResponse?> DisableTwoFa(int userId, string? code)
        {
            if (string.IsNullOrEmpty(code) || (code.Length != 6 && code.Length != 8))
                return null;

            code = code.ToUpper();
            
            var result = await VerifyOtp(userId, code);
            var resultBackup = await VerifyBackupCode(userId, Encoding.UTF8.GetBytes(code));

            if ((result is null || !result.Verification) && !resultBackup)
                return new TwoFactorVerificationResponse
                {
                    Verification = false,
                    Message = "OTP verification failed"
                };

            string query = "update two_factor_auth set is_2fa_enabled = false, encrypted_totp_secret = null, backup_codes = null where user_id = @user_id and is_2fa_enabled = true";

            Dictionary<string, object> parameters = new()
            {
                {"@user_id", userId}
            };

            if (await _dbContext.ExecuteNonQueryAsync(query, parameters) != 1)
                return new TwoFactorVerificationResponse
                {
                    Verification = false,
                    Message = "2fa could not be disabled"
                };

            return new TwoFactorVerificationResponse
            {
                Verification = true,
                Message = "2Fa disabled successfully"
            };
        }

        public async Task<TwoFactorBackupResponse?> GenerateBackupCodes(int userId)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            var backupCodes = new TwoFactorBackupCodes();
            var hashedBackupCodes = new List<string>();

            for (int i = 0; i < BackupCodeCount; i++)
            {
                byte[] randomBytes = CryptographyUtilities.GenerateSecureRandomBytes(BackupCodeLength);
                var code = new StringBuilder(BackupCodeLength);

                for (int j = 0; j < BackupCodeLength-1; j++)
                {
                    if (j == 4)
                        code.Append('-');

                    code.Append(chars[randomBytes[j] % chars.Length]);
                }

                string codeStr = code.ToString();

                byte[] hash = Sha256Hasher.ComputeByte(Encoding.UTF8.GetBytes(codeStr));

                backupCodes.Codes.Add(codeStr);
                hashedBackupCodes.Add(Convert.ToBase64String(hash));
            }

            string query = "update two_factor_auth set backup_codes = @backup_codes where user_id = @user_id and is_2fa_enabled = true and (backup_codes is null or updated_at is null or updated_at <= now() - interval 2 hour)";
            Dictionary<string, object> parameters = new()
            {
                { "@user_id", userId },
                { "@backup_codes", JsonSerializer.Serialize(hashedBackupCodes) }
            };

            if (await _dbContext.ExecuteNonQueryAsync(query, parameters) != 1)
                return new TwoFactorBackupResponse()
                {
                    Success = false
                };

            return new TwoFactorBackupResponse()
            {
                Success = true,
                BackupCodes = backupCodes,
            };
        }

        private string GenerateRandomSecretKey()
        {
            byte[] keyBytes = CryptographyUtilities.GenerateSecureRandomBytes(SecretKeyLength);
            return Base32Encoding.ToString(keyBytes);
        }

        private bool VerifyCode(byte[] secretKey, string code)
        {
            try
            {
                var totp = new Totp(secretKey);
                long timeStepMatched;
                return totp.VerifyTotp(code, out timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> VerifyBackupCode(int userId, byte[] code)
        {
            byte[] hash = Sha256Hasher.ComputeByte(code);

            string query = "select backup_codes from two_factor_auth where user_id = @user_id and is_2fa_enabled is true and backup_codes is not null";
            Dictionary<string, object> parameters = new()
            {
                { "@user_id", userId }
            };

            using var reader = await _dbContext.ExecuteReaderAsync(query, parameters);

            if (!await reader.ReadAsync())
                return false;

            var backupCodes = JsonSerializer.Deserialize<List<string>>(reader.GetString("backup_codes"));
            await reader.DisposeAsync();

            if (backupCodes is null)
                return false;

            // Find and remove used backup code
            foreach (string backupCode in backupCodes)
            {
                if (!CryptographicOperations.FixedTimeEquals(hash, Convert.FromBase64String(backupCode)))
                    continue;

                backupCodes.Remove(backupCode);

                // Update database to remove used code
                query = "update two_factor_auth set backup_codes = @backup_codes where user_id = @user_id";
                parameters.Add("backup_codes", JsonSerializer.Serialize(backupCodes));

                await _dbContext.ExecuteNonQueryAsync(query, parameters);
                return true;
            }

            return false;
        }

        private byte[] GenerateQRCodeData(string otpUri)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(otpUri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }

        private string GenerateOtpAuthUri(string userEmail, string secretKey, string appName)
        {
            return $"otpauth://totp/{Uri.EscapeDataString(appName)}:{Uri.EscapeDataString(userEmail)}?" +
                   $"secret={secretKey}&issuer={Uri.EscapeDataString(appName)}" +
                   "&algorithm=SHA1&digits=6&period=30";
        }
    }
}
