using BlockSense.Cryptography.Encryption;
using BlockSense.Cryptography.Hashing;
using BlockSenseAPI.Cryptography;
using BlockSenseAPI.Models.TwoFactorAuth;
using Org.BouncyCastle.Security;
using OtpNet;
using QRCoder;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Mysqlx.Expect.Open.Types.Condition.Types;

namespace BlockSenseAPI.Services.UserServices
{
    public interface ITwoFactorAuthService
    {
        Task<TwoFactorSetupResponseModel?> BeginSetup(int userId);
        Task<bool> CompleteSetup(int userId, TwoFactorSetupRequestModel request);
        Task<TwoFactorVerificationResponse?> VerifyOtp(int userId, string? code);
    }

    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseContext _dbContext;
        private const int _secretKeyLength = 20; // 160-bit secret for TOTP
        private const int _backupCodeCount = 5;
        private const int _backupCodeLength = 8;
        private readonly string _appName;
        private readonly string _masterKey;

        public TwoFactorAuthService(IConfiguration configuration, DatabaseContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _appName = _configuration["2FaConfig:Issuer"]!;
            _masterKey = _configuration["2FaConfig:MasterKey"]!;
        }

        public async Task<TwoFactorSetupResponseModel?> BeginSetup(int userId)
        {
            string query = "select email from users where user_id = @user_id";

            Dictionary<string, object> parameters = new()
            {
                { "@user_id", userId },
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (!await reader.ReadAsync())
                    return null;

                string secretKey = GenerateRandomSecretKey();
                string email = reader.GetString("email");
                string otpAuthUri = GenerateOtpAuthUri(secretKey, email, _appName);
                byte[] qrCodeData = GenerateQRCodeData(otpAuthUri);

                return new TwoFactorSetupResponseModel
                {
                    SetupKey = secretKey,
                    QRCodeData = qrCodeData
                };
            }
        }

        public async Task<bool> CompleteSetup(int userId, TwoFactorSetupRequestModel request)
        {
            if (request is null || request.SecretKey is null || request.Code is null || request.Code.Length != 6)
                return false;

            if (!VerifyCode(Base32Encoding.ToBytes(request.SecretKey), request.Code))
                return false;

            var backupCodes = GenerateBackupCodes();

            byte[] key = Convert.FromBase64String(_masterKey);
            byte[] nonce = Aes256Gcm.GenerateNonce();
            byte[] plaintext = Base32Encoding.ToBytes(request.SecretKey);

            byte[] ciphertext = Aes256Gcm.Encrypt(plaintext, key, nonce);

            // Combine nonce (12) + ciphertextWithTag (36) = 48 bytes
            byte[] encryptedSecret = new byte[nonce.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, encryptedSecret, 0, nonce.Length);
            Buffer.BlockCopy(ciphertext, 0, encryptedSecret, nonce.Length, ciphertext.Length);

            string query = "insert into user_authentication values(@user_id, true, @encrypted_totp_secret, @backup_codes, default)";

            Dictionary<string, object> parameters = new()
            {
                { "@user_id",  userId},
                { "@encrypted_totp_secret", encryptedSecret },
                { "@backup_codes", JsonSerializer.Serialize(backupCodes) }
            };

            if (await _dbContext.ExecuteNonQueryAsync(query, parameters) != 1)
                return false;

            return true;
        }

        public async Task<TwoFactorVerificationResponse?> VerifyOtp(int userId, string? code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
                return null;

            if (await VerifyBackupCode(userId, Encoding.UTF8.GetBytes(code)))
                return new TwoFactorVerificationResponse
                {
                    Verification = true,
                    Message = "Verified using backup code"
                };

            string query = "select encrypted_totp_secret from user_authetication where user_id = @user_id";

            Dictionary<string, object> parameters = new()
            {
                {"@user_id", userId}
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (!await reader.ReadAsync())
                    return new TwoFactorVerificationResponse
                    {
                        Verification = false,
                        Message = "2Fa not enabled for this user"
                    };

                byte[] storedData = new byte[48];
                reader.GetBytes("encrypted_totp_secret", 0, storedData, 0, 32);

                byte[] nonce = new byte[12];

                // Extract nonce (12) + ciphertextWithTag (36)
                byte[] ciphertext = new byte[storedData.Length - 12];

                Buffer.BlockCopy(storedData, 0, nonce, 0, 12);
                Buffer.BlockCopy(storedData, 12, ciphertext, 0, ciphertext.Length);

                byte[] key = Convert.FromBase64String(_masterKey);
                byte[] decryptedSecret = Aes256Gcm.Decrypt(ciphertext, key, nonce);

                if (VerifyCode(decryptedSecret, code))
                    return new TwoFactorVerificationResponse
                    {
                        Verification = true,
                        Message = "Otp verification successfull"
                    };

                return new TwoFactorVerificationResponse
                {
                    Verification = false,
                    Message = "Otp verification failed"
                };
            }
        }

        private string GenerateRandomSecretKey()
        {
            var keyBytes = new byte[_secretKeyLength];
            keyBytes = CryptographyUtils.SecureRandomGenerator(_secretKeyLength);
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
            code = HashingFunctions.ComputeSha256(code);
            string query = "select backup_codes from user_authentication where user_id = @user_id";
            Dictionary<string, object> parameters = new()
            {
                { "@user_id", userId }
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (!await reader.ReadAsync())
                    return false;

                var backupCodes = JsonSerializer.Deserialize<List<string>>(reader.GetString("backup_codes"));

                if (backupCodes is null)
                    return false;

                // Find and remove used backup code
                foreach (string backupCode in backupCodes)
                {
                    if (!CryptographicOperations.FixedTimeEquals(code, Convert.FromBase64String(backupCode)))
                        continue;

                    backupCodes.Remove(backupCode);

                    // Update database to remove used code
                    query = "update user_authentication set backup_codes = @backup_codes where user_id = @user_id";

                    parameters.Add("backup_codes", backupCodes);

                    await _dbContext.ExecuteNonQueryAsync(query, parameters);
                    return true;
                }
                
                return false;
            }
        }

        private byte[] GenerateQRCodeData(string otpUri)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(otpUri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }

        private string GenerateOtpAuthUri(string secretKey, string userEmail, string appName)
        {
            return $"otpauth://totp/{Uri.EscapeDataString(appName)}:{Uri.EscapeDataString(userEmail)}?" +
                   $"secret={secretKey}&issuer={Uri.EscapeDataString(appName)}" +
                   "&algorithm=SHA1&digits=6&period=30";
        }

        private List<string> GenerateBackupCodes()
        {
            var backupCodes = new List<string>();
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // No confusing chars

            for (int i = 0; i < _backupCodeCount; i++)
            {
                byte[] randomBytes = CryptographyUtils.SecureRandomGenerator(_backupCodeLength);
                var code = new StringBuilder(_backupCodeLength);

                for (int j = 0; j < _backupCodeLength; j++)
                {
                    code.Append(chars[randomBytes[j] % chars.Length]);
                }

                string codeStr = code.ToString();
                byte[] hash = HashingFunctions.ComputeSha256(Encoding.UTF8.GetBytes(codeStr));

                backupCodes.Add(Convert.ToBase64String(hash));
            }
            return backupCodes;
        }
    }
}
