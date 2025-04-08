using BlockSense.Client.Identifiers;
using BlockSense.Server;
using BlockSense.Server.Cryptography.Hashing;
using BlockSense.Server.Cryptography.TokenAuthentication;
using Org.BouncyCastle.Crypto.Paddings;
using System;
using BlockSense.DatabaseUtils;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Server_Based.Cryptography.Token_authentication.Refresh_Token
{
    class RemoteRefreshToken
    {
        public static byte[] PlainToken { get; private set; } = null!;
        public static Guid TokenId { get; private set; }
        public static DateTime IssuedAt { get; private set; }
        public static DateTime ExpiresAt { get; private set; }
        
        /// <summary>
        /// Generates a new token
        /// </summary>
        public static void Generate()
        {
            PlainToken = HashUtils.SecureRandomGenerator(32);
            TokenId = Guid.NewGuid();
            IssuedAt = DateTime.UtcNow;
            ExpiresAt = IssuedAt.AddDays(3);
        }

        /// <summary>
        /// Server simulated API
        /// </summary>
        /// <returns>TokenChace object filled with token data</returns>
        public static TokenCache GetTokenData()
        {
            return new TokenCache()
            {
                TokenId = TokenId,
                IssuedAt = IssuedAt,
                ExpiresAt = ExpiresAt,
                EncryptedData = PlainToken
            };
        }

        /// <summary>
        /// Stores hashed Token, IP address, device id, Issuance & Expiration Date
        /// </summary>
        /// <param name="refreshToken">plain refresh token</param>
        /// <returns></returns>
        public static async Task Store()
        {
            string hashedToken = Convert.ToBase64String(HashUtils.ComputeSha256(PlainToken));
            Dictionary<string, object> parameters = new()
            {
                {"@refreshtoken_id", TokenId},
                {"@user_id", User.Uid},
                {"@refresh_token", hashedToken},
                {"@hardware_identifier", HardwareIdentifier.HardwareId},
                {"@network_identifier", NetworkIdentifier.MacAddress},
                {"@ip_address", NetworkIdentifier.IpAddress},
                {"@device_identifier", HardwareIdentifier.DeviceId},
                {"@issued_at", IssuedAt},
                {"@expires_at", ExpiresAt}
            };

            string query = "insert into refreshtokens values (@refreshtoken_id, @user_id, @refresh_token, @hardware_identifier, @network_identifier, @ip_address, @device_identifier, @issued_at, @expires_at, default)";
            if (await Database.StoreData(query, parameters))
            {
                ConsoleHelper.Log("Refresh token stored successfully");
            }
        }

        /// <summary>
        /// Sets hashed Token & expiration Date
        /// </summary>
        /// <returns>fundation of valid token</returns>
        public static async Task<byte[]> Fetch(Guid tokenId)
        {
            Dictionary<string, object> parameters = new()
                {
                    {"@refreshtoken_id", tokenId},
                    {"@ip_address", NetworkIdentifier.IpAddress},
                    {"@device_identifier", HardwareIdentifier.DeviceId}
                };
            string query = "select refreshtoken_id, refresh_token, ip_address, device_identifier, expires_at, revoked from refreshtokens where refreshtoken_id = @refreshtoken_id and revoked = 0";
            using (var reader = await Database.FetchData(query, parameters))
            {
                if (reader.Read())
                {
                    // Check if the token is expired
                    if (reader.GetDateTime("expires_at") < DateTime.UtcNow)
                    {
                        await TokenUtils.Revoke(tokenId);
                        ConsoleHelper.Log("Token expired");
                        return Array.Empty<byte>();
                    }

                    return Convert.FromBase64String(reader.GetString("refresh_token"));
                }
                return Array.Empty<byte>();
            }
        }
    }
}
