using BlockSense.Cryptography.Hashing;
using BlockSense.Models.Responses;
using BlockSense.Models.Token;
using BlockSenseAPI.Cryptography;
using BlockSenseAPI.Models;
using BlockSenseAPI.Models.Requests;
using BlockSenseAPI.Models.Token;
using BlockSenseAPI.Services.UserServices;
using System.Data;
using System.Security.Cryptography;

namespace BlockSenseAPI.Services.TokenServices
{
    public interface IRefreshTokenService
    {
        RefreshTokenModel GenerateRefreshToken(int userId);
        Task StoreRefreshToken(TokenRefreshRequestModel request);
        Task<TokenRefreshResponseModel?> RefreshAccessToken(TokenRefreshRequestModel request);
        Task RevokeRefreshToken(Guid tokenId);
    }

    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly DatabaseContext _dbContext;
        private readonly IAccessTokenService _accessTokenService;
        private readonly ValidatorService _validatorService;
        private readonly GeoLookupService _geoLookupService;

        public RefreshTokenService(DatabaseContext dbContext, IAccessTokenService accessTokenService, ValidatorService validatorService, GeoLookupService geoLookupService)
        {
            _dbContext = dbContext;
            _accessTokenService = accessTokenService;
            _validatorService = validatorService;
            _geoLookupService = geoLookupService;
        }

        public RefreshTokenModel GenerateRefreshToken(int userId)
        {
            byte[] plainToken = CryptographyUtils.SecureRandomGenerator(32);
            Guid tokenId = Guid.NewGuid();
            DateTime issuedAt = DateTime.UtcNow;
            DateTime expiresAt = issuedAt.AddDays(3);

            return new RefreshTokenModel
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
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task StoreRefreshToken(TokenRefreshRequestModel request)
        {
            if (request is null || request.RefreshToken is null || request.Identifiers is null)
                return;

            string hashedToken = Convert.ToBase64String(HashingFunctions.ComputeSha256(request.RefreshToken.Data));

            string query = "insert into refresh_tokens values (@token_id, @user_id, @token_hash, @hardware_fingerprint, @network_fingerprint, @ip_address, @device_identifier, @issued_at, @expires_at, default)";
            Dictionary<string, object?> parameters = new()
            {
                {"@token_id", request.RefreshToken.TokenId},
                {"@user_id", request.RefreshToken.UserId},
                {"@token_hash", hashedToken},
                {"@hardware_fingerprint", request.Identifiers.HardwareId},
                {"@network_fingerprint", request.Identifiers.MacAddress},
                {"@ip_address", request.Identifiers.IpAddress},
                {"@device_identifier", request.Identifiers.DeviceId},
                {"@issued_at", request.RefreshToken.IssuedAt},
                {"@expires_at", request.RefreshToken.ExpiresAt}
            };

            await _dbContext.ExecuteNonQueryAsync(query, parameters!);
            _dbContext.Dispose();
        }

        /// <summary>
        /// Fetches a hashed refresh Token
        /// </summary>
        /// <param name="tokenId">Guid of requested refresh Token</param>
        /// <returns>hashed refresh Token</returns>
        private async Task<(RefreshTokenModel? token, string message)> FetchRefreshToken(Guid tokenId)
        {
            string query = "select user_id, token_hash, issued_at, expires_at, is_revoked from refresh_tokens where token_id = @token_id";
            Dictionary<string, object> parameters = new()
            {
                {"@token_id", tokenId}
            };

            using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
            {
                if (await reader.ReadAsync())
                {
                    if (reader.GetBoolean("is_revoked"))
                        return (null, "Token revoked");

                    if (reader.GetDateTime("expires_at") < DateTime.UtcNow)
                        return (null, "Token expired");

                    var refreshToken = new RefreshTokenModel
                    {
                        TokenId = tokenId,
                        UserId = reader.GetInt32("user_id"),
                        Data = Convert.FromBase64String(reader.GetString("token_hash")),
                        IssuedAt = reader.GetDateTime("issued_at"),
                        ExpiresAt = reader.GetDateTime("expires_at")
                    };

                    return (refreshToken, "Token fetched successfully");
                }

                return (null, "Token not found");
            }
        }

        /// <summary>
        /// Comparison between locally stored and valid refresh Tokens, including GeoLookup and system Identifiers check
        /// </summary>
        /// <param name="request"></param>
        /// <returns>boolean value of comparison</returns>
        public async Task<TokenRefreshResponseModel?> RefreshAccessToken(TokenRefreshRequestModel request)
        {
            if (request is null || request.RefreshToken is null || request.Identifiers is null)
                return null;

            try
            {
                SystemIdentifierModel validIdentifiers;
                var (validToken, message) = await FetchRefreshToken(request.RefreshToken.TokenId);

                if (validToken is null)
                    return new TokenRefreshResponseModel
                    {
                        Success = false,
                        Message = message
                    };

                string query = "select hardware_fingerprint, network_fingerprint, ip_address from refresh_tokens where token_id = @token_id";
                Dictionary<string, object> parameters = new()
                {
                    {"@token_id", request.RefreshToken.TokenId },
                    {"@user_id", request.RefreshToken.UserId }
                };
                using (var reader = await _dbContext.ExecuteReaderAsync(query, parameters))
                {
                    if (!await reader.ReadAsync())
                        return null;

                    validIdentifiers = new SystemIdentifierModel
                    {
                        HardwareId = reader.GetString("hardware_fingerprint"),
                        MacAddress = reader.GetString("network_fingerprint"),
                        IpAddress = reader.GetString("ip_address")
                    };
                }


                var validatorService = new ValidatorService(request.Identifiers, validIdentifiers, _geoLookupService);

                if (!validatorService.GetResult())
                    return new TokenRefreshResponseModel
                    {
                        Success = false,
                        Message = "Identifiers do not match"
                    };

                // Hash the decrypted token
                byte[] hashedClientToken = HashingFunctions.ComputeSha256(request.RefreshToken.Data);

                if (CryptographicOperations.FixedTimeEquals(hashedClientToken, validToken.Data))
                {
                    return new TokenRefreshResponseModel
                    {
                        Success = true,
                        Message = "Refresh successfull",
                        AccessToken = _accessTokenService.GenerateAccessToken(request.RefreshToken.UserId)
                    };
                }

                return new TokenRefreshResponseModel
                {
                    Success = false,
                    Message = "Refresh tokens is not valid",
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task RevokeRefreshToken(Guid tokenId)
        {
            string query = "update refresh_tokens set is_revoked = true where token_id = @token_id";
            Dictionary<string, object> parameters = new()
            {
                {"@token_id", tokenId}
            };

            await _dbContext.ExecuteNonQueryAsync(query, parameters);
            _dbContext.Dispose();
        }
    }
}
