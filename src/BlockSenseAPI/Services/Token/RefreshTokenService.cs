using BlockSenseAPI.Cryptography;
using BlockSenseAPI.Cryptography.Hashing;
using BlockSenseAPI.Models;
using BlockSenseAPI.Models.Token;
using BlockSenseAPI.Models.Token.Configs;
using BlockSenseAPI.Models.Token.DTOs;
using BlockSenseAPI.Services.SystemValidation;
using Microsoft.Extensions.Options;
using System.Data;
using System.Security.Cryptography;

namespace BlockSenseAPI.Services.Token
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly RefreshTokenConfig _config;
        private readonly DatabaseContext _dbContext;
        private readonly IAccessTokenService _accessTokenService;

        public RefreshTokenService(IOptions<RefreshTokenConfig> refreshTokenConfig, DatabaseContext dbContext, IAccessTokenService accessTokenService)
        {
            _config = refreshTokenConfig.Value;
            _dbContext = dbContext;
            _accessTokenService = accessTokenService;
        }

        public RefreshToken GenerateRefreshToken(int userId)
        {
            byte[] plainToken = CryptographyUtilities.GenerateSecureRandomBytes(32);
            Guid tokenId = Guid.NewGuid();
            DateTime issuedAt = DateTime.UtcNow;
            DateTime expiresAt = issuedAt.AddDays(_config.RefreshTokenExpirationDays);

            return new RefreshToken
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
        public async Task StoreRefreshToken(TokenRefreshRequest request)
        {
            if (request is null || request.RefreshToken is null || request.Identifiers is null)
                return;

            string hashedToken = Convert.ToBase64String(Sha256Hasher.ComputeByte(request.RefreshToken.Data));

            string query = "insert into refresh_tokens values (@token_id, @user_id, @token_hash, @hardware_fingerprint, @network_fingerprint, @ip_address, @device_identifier, @issued_at, @expires_at, default)" +
                "on duplicate key update token_id = values(token_id), token_hash = values(token_hash),user_id = values(user_id),ip_address = values(ip_address),device_identifier = values(device_identifier), " +
                "issued_at = values(issued_at),expires_at = values(expires_at),is_revoked = values(is_revoked)";
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
        private async Task<(RefreshToken? token, string message)> FetchRefreshToken(Guid tokenId)
        {
            string query = "select user_id, token_hash, issued_at, expires_at, is_revoked from refresh_tokens where token_id = @token_id";
            Dictionary<string, object> parameters = new()
            {
                {"@token_id", tokenId}
            };

            using var reader = await _dbContext.ExecuteReaderAsync(query, parameters);

            if (!await reader.ReadAsync())
                return (null, "Token not found");

            if (reader.GetBoolean("is_revoked"))
                return (null, "Token revoked");

            if (reader.GetDateTime("expires_at") < DateTime.UtcNow)
                return (null, "Token expired");

            var refreshToken = new RefreshToken
            {
                TokenId = tokenId,
                UserId = reader.GetInt32("user_id"),
                Data = Convert.FromBase64String(reader.GetString("token_hash")),
                IssuedAt = reader.GetDateTime("issued_at"),
                ExpiresAt = reader.GetDateTime("expires_at")
            };

            return (refreshToken, "Token fetched successfully");
        }

        /// <summary>
        /// Comparison between locally stored and valid refresh Tokens, including GeoLookup and system Identifiers check
        /// </summary>
        /// <param name="request"></param>
        /// <returns>boolean value of comparison</returns>
        public async Task<TokenRefreshResponse?> RefreshAccessToken(TokenRefreshRequest request)
        {
            if (request is null || request.RefreshToken is null || request.Identifiers is null)
                return null;

            try
            {
                var (validToken, message) = await FetchRefreshToken(request.RefreshToken.TokenId);

                if (validToken is null)
                    return new TokenRefreshResponse
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

                using var reader = await _dbContext.ExecuteReaderAsync(query, parameters);

                if (!await reader.ReadAsync())
                    return null;

                SystemIdentifier validIdentifiers = new SystemIdentifier
                {
                    HardwareId = reader.GetString("hardware_fingerprint"),
                    MacAddress = reader.GetString("network_fingerprint"),
                    IpAddress = reader.GetString("ip_address")
                };

                var validatorService = new SystemValidationService(request.Identifiers, validIdentifiers);

                // Hash the decrypted token
                byte[] hashedClientToken = Sha256Hasher.ComputeByte(request.RefreshToken.Data);

                if (!CryptographicOperations.FixedTimeEquals(hashedClientToken, validToken.Data))
                    return new TokenRefreshResponse
                    {
                        Success = false,
                        Message = "Refresh tokens is not valid",
                    };

                var identValidation = validatorService.GetResult();

                if (!identValidation.Success)
                    return new TokenRefreshResponse
                    {
                        Success = false,
                        Message = identValidation.Message
                    };

                return new TokenRefreshResponse
                {
                    Success = true,
                    Message = "Refresh successfull",
                    AccessToken = _accessTokenService.GenerateAccessToken(request.RefreshToken.UserId)
                };
            }
            catch (Exception ex)
            {
                return new TokenRefreshResponse
                {
                    Success = false,
                    Message = ex.Message
                };
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
