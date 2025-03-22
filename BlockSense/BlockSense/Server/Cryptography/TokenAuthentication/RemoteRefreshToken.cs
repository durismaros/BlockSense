using BlockSense.DB;
using BlockSense.Server.Cryptography.Hashing;
using BlockSense.Server.Cryptography.TokenAuthentication;
using BlockSense.Server.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Server_Based.Cryptography.Token_authentication.Refresh_Token
{
    class RemoteRefreshToken
    {
        private static string _hashedToken = string.Empty;
        public static int tokenId;
        private static DateTime _expiresAt;

        /// <summary>
        /// Stores hashed Token, IP address, device id, Issuance & Expiration Date
        /// </summary>
        /// <param name="refreshToken">plain refresh token</param>
        /// <returns></returns>
        public static async Task Store(string refreshToken)
        {
            string hashedRefreshToken = HashUtils.GenerateHash(refreshToken, null);
            DateTime timeStamp = DateTime.UtcNow;
            Dictionary<string, string> parameters = new()
            {
                {"@user_id", User.Uid},
                {"@ip_address", User.ipAddress},
                {"@refresh_token", hashedRefreshToken},
                {"@device_identifier", User.deviceIdentifier},
                {"@issued_at", timeStamp.ToString("yyyy-MM-dd HH:mm:ss")},
                {"@expires_at", timeStamp.AddDays(3).ToString("yyyy-MM-dd HH:mm:ss")}
            };

            string query = "INSERT INTO refreshtokens (user_id, refresh_token, ip_address, device_identifier, issued_at, expires_at) " +
                           "VALUES (@user_id, @refresh_token, @ip_address, @device_identifier, @issued_at, @expires_at)";
            if (await Database.StoreData(query, parameters))
            {
                ConsoleHelper.WriteLine("Refresh token stored successfully");
            }
        }

        /// <summary>
        /// Sets hashed Token & expiration Date
        /// </summary>
        /// <returns>fundation of valid token</returns>
        public static async Task<bool> Fetch()
        {
            Dictionary<string, string> parameters = new()
                {
                    {"@user_id", User.Uid},
                    {"@ip_address", User.ipAddress},
                    {"@device_identifier", User.deviceIdentifier}
                };
            string query = "SELECT refreshtoken_id, refresh_token, ip_address, device_identifier, expires_at, revoked FROM refreshtokens WHERE user_id = @user_id AND ip_address = @ip_address AND device_identifier = @device_identifier AND revoked = 0";
            using (var reader = await Database.FetchData(query, parameters))
            {
                if (reader.Read())
                {
                    _hashedToken = reader.GetString("refresh_token");
                    _expiresAt = reader.GetDateTime("expires_at");
                    tokenId = reader.GetInt32("refreshtoken_id");


                    // Check if the token is expired
                    if (DateTime.UtcNow > _expiresAt)
                    {
                        await Revoke(tokenId);
                        return false;
                    }

                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Revokes specified user refresh Token
        /// </summary>
        /// <param name="refreshTokenId">id of desired Token</param>
        /// <returns></returns>
        public static async Task Revoke(int refreshTokenId)
        {
            Dictionary<string, string> parameters = new()
            {
                {"@user_id", User.Uid},
                {"@refreshtoken_id", refreshTokenId.ToString()}
            };
            string query = "UPDATE refreshtokens set revoked = 1 WHERE user_id = @user_id AND refreshtoken_id = @refreshtoken_id AND revoked = 0";
            await Database.StoreData(query, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptedRefreshToken"></param>
        /// <returns>Comparison between locally stored and valid Token</returns>
        public static async Task<bool> Comparison(string encryptedRefreshToken)
        {
            try
            {
                // Fetch encryption key and IV for the user
                var (encryptionKey, initializationVector) = await EncryptionKey.Fetch();
                // Decrypt the refresh token sent by the client
                string plainToken = Encryption.DecryptRefreshToken(encryptedRefreshToken, encryptionKey, initializationVector);
                // Hash the decrypted token
                string clientHashedToken = HashUtils.GenerateHash(plainToken, null);

                if (await RemoteRefreshToken.Fetch() && clientHashedToken.Equals(_hashedToken))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine("Error: " + ex.Message);
                throw;
            }
        }
    }
}
