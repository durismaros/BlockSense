using BlockSenseAPI.Models;
using BlockSenseAPI.Services.Cryptography;
using System.Data;

namespace BlockSenseAPI.Services
{
    public interface IRefreshTokenService
    {
        TokenCache GenerateRefreshToken(int userId);
        Task StoreRefreshToken(TokenCache tokenCache, SystemIdentifiers identifiers);
        Task<(byte[] token, string message)> FetchRefreshToken(Guid tokenId);
        Task RevokeRefreshToken(Guid tokenId);
    }

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly DatabaseContext _dbContext;

        public RefreshTokenService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public TokenCache GenerateRefreshToken(int userId)
        {
            byte[] plainToken = CryptoUtils.SecureRandomGenerator(32);
            Guid tokenId = Guid.NewGuid();
            DateTime issuedAt = DateTime.UtcNow;
            DateTime expiresAt = issuedAt.AddDays(3);

            return new TokenCache()
            {
                TokenId = tokenId,
                UserId = userId,
                Data = plainToken,
                IssuedAt = issuedAt,
                ExpiresAt = expiresAt
            };
        }


        /// <summary>
        /// Stores hashed refresh Token, Hardware and Network identifiers together with Issuance & Expiration date
        /// </summary>
        /// <param name="tokenCache">refresh Token object</param>
        /// <param name="hardwareIdents">Hardware identifiers instance</param>
        /// <param name="networkIdents">Network identifiers instance</param>
        /// <returns></returns>
        public async Task StoreRefreshToken(TokenCache tokenCache, SystemIdentifiers identifiers)
        {
            string hashedToken = Convert.ToBase64String(CryptoUtils.ComputeSha256(tokenCache.Data));

            string query = "insert into refreshtokens values (@refreshtoken_id, @user_id, @refresh_token, @hardware_identifier, @network_identifier, @ip_address, @device_identifier, @issued_at, @expires_at, default)";
            Dictionary<string, object> parameters = new()
            {
                {"@refreshtoken_id", tokenCache.TokenId},
                {"@user_id", tokenCache.UserId},
                {"@refresh_token", hashedToken},
                {"@hardware_identifier", identifiers.HardwareId},
                {"@network_identifier", identifiers.MacAddress},
                {"@ip_address", identifiers.IpAddress},
                {"@device_identifier", identifiers.DeviceId},
                {"@issued_at", tokenCache.IssuedAt},
                {"@expires_at", tokenCache.ExpiresAt}
            };

            await _dbContext.ExecuteNonQueryAsync(query, parameters);
            _dbContext.Dispose();

        }


        /// <summary>
        /// Fetches a hashed refresh Token
        /// </summary>
        /// <param name="tokenId">Guid of requested refresh Token</param>
        /// <returns>hashed refresh Token</returns>
        public async Task<(byte[] token, string message)> FetchRefreshToken(Guid tokenId)
        {
            string query = "select refresh_token, expires_at, revoked from refreshtokens where refreshtoken_id = @refreshtoken_id and revoked = false";
            Dictionary<string, object> parameters = new()
            {
                {"@refreshtoken_id", tokenId}
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (await reader.ReadAsync())
                {
                    if (reader.GetBoolean("revoked"))
                        return (Array.Empty<byte>(), "Token revoked");

                    if (reader.GetDateTime("expires_at") < DateTime.UtcNow)
                        return (Array.Empty<byte>(), "Token expired");

                    byte[] refreshToken = Convert.FromBase64String(reader.GetString("refresh_token"));
                    return (refreshToken, "Token fetched successfully");
                }

                return (Array.Empty<byte>(), "Token not found");
            }
        }

        public async Task RevokeRefreshToken(Guid tokenId)
        {
            string query = "update refreshtokens set revoked = true WHERE refreshtoken_id = @refreshtoken_id";
            Dictionary<string, object> parameters = new()
            {
                {"@refreshtoken_id", tokenId}
            };

            await _dbContext.ExecuteNonQueryAsync(query, parameters);
            _dbContext.Dispose();
        }
    }
}
