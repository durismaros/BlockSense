using System;
using BlockSense.DatabaseUtils;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BlockSense.Client.Cryptography.Hashing;

namespace BlockSense.Server.Cryptography.TokenAuthentication
{
    class TokenUtils
    {
        /// <summary>
        /// Revokes specified user refresh Token
        /// </summary>
        /// <param name="refreshTokenId">id of desired Token</param>
        /// <returns></returns>
        public static async Task Revoke(Guid tokenID)
        {
            Dictionary<string, object> parameters = new()
            {
                {"@refreshtoken_id", tokenID}
            };
            string query = "update refreshtokens set revoked = 1 where refreshtoken_id = @refreshtoken_id and revoked = 0";
            await Database.StoreData(query, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptedRefreshToken"></param>
        /// <returns>Comparison between locally stored and valid Token</returns>
        public static async Task<bool> Comparison(byte[] plainToken, Guid tokenId)
        {
            try
            {
                var validator = await FetchIdentsFromToken(tokenId);

                //validator.CurrentHwid = HardwareIdentifiers.HardwareId;
                //validator.CurrentMacAddress = NetworkIdentifiers.MacAddress;
                //validator.CurrentIpAddress = NetworkIdentifiers.IpAddress;

                if (!validator.GetResult())
                    return false;

                // Hash the decrypted token
                byte[] hashedClientToken = HashUtils.ComputeSha256(plainToken);
                //byte[] hashedRemoteToken = await RemoteRefreshToken.Fetch(tokenId);

                //if (CryptographicOperations.FixedTimeEquals(hashedClientToken, hashedRemoteToken))
                //    return true;

                return false;
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
                return false;
            }
        }

        public static async Task<int> FetchUidFromToken(Guid tokenId)
        {
            Dictionary<string, object> parameters = new()
            {
                {"@refreshtoken_id", tokenId}
            };
            string query = "select user_id from refreshtokens where refreshtoken_id = @refreshtoken_id";
            using (var reader = await Database.FetchData(query, parameters))
            {
                if (reader.Read()) return reader.GetInt32("user_id");
            }
            return 0;
        }

        public static async Task<IdentsValidator> FetchIdentsFromToken(Guid tokenId)
        {
            Dictionary<string, object> parameters = new()
            {
                {"@refreshtoken_id", tokenId}
            };
            string query = "select hardware_identifier, network_identifier, ip_address, device_identifier from refreshtokens where refreshtoken_id = @refreshtoken_id";
            using (var reader = await Database.FetchData(query, parameters))
            {
                if (reader.Read())
                {
                    return new IdentsValidator()
                    {
                        StoredHwid = reader.GetString("hardware_identifier"),
                        StoredMacAddress = reader.GetString("network_identifier"),
                        StoredIpAddress = reader.GetString("ip_address")
                    };
                }
            }
            return new IdentsValidator();
        }
    }
}
