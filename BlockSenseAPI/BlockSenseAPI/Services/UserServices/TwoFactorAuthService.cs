using BlockSense.Cryptography.Encryption;
using BlockSenseAPI.Cryptography;
using BlockSenseAPI.Models.TwoFactorAuth;
using OtpNet;
using QRCoder;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
            string query = "select email from users where uid = @uid";
            string secretKey = GenerateRandomSecretKey();
            string otpAuthUri;

            Dictionary<string, object> parameters = new()
            {
                { "uid", userId },
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (!await reader.ReadAsync())
                    return null;

                otpAuthUri = GenerateOtpAuthUri(secretKey, reader.GetString("email"), _appName);
            }
            var qrCodeData = GenerateQRCodeData(otpAuthUri);

            return new TwoFactorSetupResponseModel
            {
                SetupKey = secretKey,
                QRCodeData = qrCodeData
            };
        }

        public async Task<bool> CompleteSetup(int userId, TwoFactorSetupRequestModel request)
        {
            if (request is null || request.SecretKey is null || request.Code is null || request.Code.Length != 6)
                return false;

            if (!VerifyCode(Base32Encoding.ToBytes(request.SecretKey), request.Code))
                return false;

            byte[] key = Convert.FromBase64String(_masterKey);
            byte[] nonce = Aes256Gcm.GenerateNonce();
            byte[] cipherText = Aes256Gcm.Encrypt(Base32Encoding.ToBytes(request.SecretKey), key, nonce);

            var storageFormat = new byte[nonce.Length + cipherText.Length];
            Buffer.BlockCopy(nonce, 0, storageFormat, 0, nonce.Length);
            Buffer.BlockCopy(cipherText, 0, storageFormat, nonce.Length, cipherText.Length);

            string query = "insert into user_2fa_auth values(@uid, @secret_key, default, default)";

            Dictionary<string, object> parameters = new()
            {
                { "uid",  userId},
                { "secret_key", storageFormat },
            };

            await _dbContext.ExecuteNonQueryAsync(query, parameters);
            _dbContext.Dispose();

            return true;
        }

        public async Task<TwoFactorVerificationResponse?> VerifyOtp(int userId, string? code)
        {
            if (code is null || code.Length != 6)
                return null;

            string query = "select secret_key from user_2fa_auth where user_id = @uid";

            Dictionary<string, object> parameters = new()
            {
                {"uid", userId}
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (!await reader.ReadAsync())
                    return new TwoFactorVerificationResponse
                    {
                        Verification = false,
                        Message = "Couldn't find the specified user"
                    };



                byte[] storedData = new byte[48];
                reader.GetBytes("password", 0, storedData, 0, 32);

                byte[] key = Convert.FromBase64String(_masterKey);
                byte[] nonce = new byte[12];
                byte[] ciphertext = new byte[storedData.Length - 12];

                Buffer.BlockCopy(storedData, 0, nonce, 0, 12);
                Buffer.BlockCopy(storedData, 12, ciphertext, 0, ciphertext.Length);

                byte[] decryptedKey = Aes256Gcm.Decrypt(ciphertext, key, nonce);

                if (VerifyCode(decryptedKey, code))
                    return new TwoFactorVerificationResponse
                    {
                        Verification = true,
                        Message = "Otp verification successfull"
                    };

                return new TwoFactorVerificationResponse
                {
                    Verification = false,
                    Message = "Otp verification unsuccessfull"
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
    }
}
