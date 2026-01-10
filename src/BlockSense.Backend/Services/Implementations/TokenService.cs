using BlockSense.Backend.Data;
using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Models;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.Cryptography.Utils;
using BlockSense.Contracts.DTOs.Token;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlockSense.Backend.Services.Implementations
{
    public sealed class TokenService : ITokenService
    {
        private readonly RefreshTokenConfig _refreshTokenConfig;
        private readonly JwtTokenConfig _jwtTokenConfig;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public TokenService(
            IOptions<RefreshTokenConfig> refreshTokenConfig,
            IOptions<JwtTokenConfig> jwtTokenConfig,
            IRefreshTokenRepository refreshTokenRepository,
            DatabaseContext databaseContext)
        {
            _refreshTokenConfig = refreshTokenConfig.Value ?? throw new ArgumentNullException(nameof(refreshTokenConfig));
            _jwtTokenConfig = jwtTokenConfig.Value ?? throw new ArgumentNullException(nameof(jwtTokenConfig));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        }
        public async Task<RefreshTokenDto> CreateRefreshTokenAsync(uint userId, DeviceContext deviceContext, CancellationToken cancellationToken = default)
        {
            Guid tokenId = Guid.NewGuid();

            byte[] rawToken = CryptographyUtilities.GenerateSecureRandomBytes(32);

            string tokenHash = Sha256Hasher.ComputeBase64(rawToken);

            var now = DateTime.UtcNow;
            var expiration = now.Add(_refreshTokenConfig.Expiration);

            var refreshTokenEntity = new RefreshTokenEntity
            {
                TokenId = tokenId,
                UserId = userId,
                TokenHash = tokenHash,
                IpAddress = deviceContext.IpAddress,
                DeviceIdentifier = deviceContext.DeviceIdentifier,
                HardwareFingerprint = deviceContext.HardwareFingerprint,
                NetworkFingerprint = deviceContext.NetworkFingerprint,
                DeviceOs = deviceContext.DeviceOs,
                IssuedAt = now,
                ExpiresAt = expiration,
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateAsync(refreshTokenEntity, cancellationToken);

            return new RefreshTokenDto
            {
                TokenId = tokenId,
                Token = Convert.ToBase64String(rawToken),
                UserId = userId,
                IssuedAt = now,
                ExpiresAt = expiration
            };
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            string tokenHash;
            try
            {
                tokenHash = Sha256Hasher.ComputeBase64(Convert.FromBase64String(token));
            }
            catch
            {
                return false;
            }

            var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(tokenHash, cancellationToken);

            return tokenEntity != null && !tokenEntity.IsRevoked && tokenEntity.ExpiresAt > DateTime.UtcNow;
        }

        public async Task<AccessTokenDto> CreateAccessTokenAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            byte[] key = Convert.FromBase64String(_jwtTokenConfig.SigningKey);
            DateTime tokenExpiry = DateTime.UtcNow.Add(_jwtTokenConfig.Expiration);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Typ, user.UserType.ToString())
                }),
                Expires = tokenExpiry,
                Issuer = _jwtTokenConfig.Issuer,
                Audience = _jwtTokenConfig.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AccessTokenDto
            {
                Token = tokenHandler.WriteToken(token),
                ExpiresAt = tokenExpiry
            };
        }
    }
}
