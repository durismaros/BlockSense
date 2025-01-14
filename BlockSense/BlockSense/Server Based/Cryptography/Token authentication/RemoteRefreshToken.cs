using Avalonia.Input;
using Avalonia.Media;
using BlockSense.Client_Side.Token_authentication;
using BlockSense.DB;
using BlockSense.Server_Based.Cryptography.Tokens;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Quic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace BlockSense.Server_Based.Cryptography.Token_authentication.Refresh_Token
{
    class RemoteRefreshToken
    {
        protected static string _hashedToken, _ipAddress, _deviceIdentifier;
        private static DateTime _expiresAt;

        public static async Task Store(string userId, string refreshToken)
        {
            string hashedRefreshToken = HashUtils.GenerateHash(refreshToken, null);
            DateTime timeStamp = DateTime.UtcNow;
            Dictionary<string, string> parameters = new()
            {
                {"@user_id", userId},
                {"@ip_address", User.ipAddress},
                {"@refresh_token", hashedRefreshToken},
                {"@device_identifier", User.deviceIdentifier},
                {"@issued_at", timeStamp.ToString("yyyy-MM-dd HH:mm:ss")},
                {"@expires_at", timeStamp.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss")}
            };

            string query = "INSERT INTO refreshtokens (user_id, refresh_token, ip_address, device_identifier, issued_at, expires_at) " +
                           "VALUES (@user_id, @refresh_token, @ip_address, @device_identifier, @issued_at, @expires_at)";
            if (await Database.StoreData(query, parameters))
            {
                ConsoleHelper.WriteLine("Refresh token stored successfully");
            }
        }

        public static async Task<bool> Fetch(string userId)
        {
            Dictionary<string, string> parameters = new()
                {
                    {"@user_id", userId}
                };
            string query = "SELECT refresh_token, ip_address, device_identifier, expires_at, revoked FROM refreshtokens WHERE user_id = @user_id AND revoked = 0";
            using (var reader = await Database.FetchData(query, parameters))
            {
                if (reader.Read())
                {
                    _hashedToken = reader.GetString("refresh_token");
                    _ipAddress = reader.GetString("ip_address");
                    _deviceIdentifier = reader.GetString("device_identifier");
                    _expiresAt = reader.GetDateTime("expires_at");

                    // Check if the token is expired
                    if (DateTime.UtcNow > _expiresAt) return false;

                    return true;
                }
                return false;
            }
        }

        public static async Task Revoke(string userId)
        {
            Dictionary<string, string> parameters = new()
            {
                {"@user_id", userId}
            };
            string query = "UPDATE refreshtokens set revoked = 1 WHERE user_id = @user_id AND revoked = 0";
            await Database.StoreData(query, parameters);
        }


        public static async Task<bool> Comparison(string userId, string encryptedRefreshToken)
        {
            try
            {
                // Fetch encryption key and IV for the user
                var (encryptionKey, initializationVector) = await EncryptionKey.Fetch(userId);
                // Decrypt the refresh token sent by the client
                string plainToken = TokenDecryption.DecryptRefreshToken(encryptedRefreshToken, encryptionKey, initializationVector);

                // Hash the decrypted token
                string clientHashedToken = HashUtils.GenerateHash(plainToken, null);

                if (await RemoteRefreshToken.Fetch(userId) && clientHashedToken.Equals(_hashedToken) && User.ipAddress.Equals(_ipAddress) && User.deviceIdentifier.Equals(_deviceIdentifier))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine("Error: " + ex.Message);
                return false;
            }
        }


        public static string GeneratePlain(int byteLength = 32)
        {
            // Create an instance of SecureRandom from Bouncy Castle
            SecureRandom random = new SecureRandom();

            // Generate a secure random byte array of the specified length
            byte[] tokenBytes = new byte[byteLength];
            random.NextBytes(tokenBytes);

            string plainToken = Convert.ToBase64String(tokenBytes);

            return plainToken;
        }
    }
}
