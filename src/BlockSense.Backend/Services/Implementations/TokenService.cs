using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Exceptions.TwoFactorAuthentication;
using BlockSense.Backend.Exceptions.User;
using BlockSense.Backend.Models.DeviceContext;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.Cryptography.Utilities;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.Token;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlockSense.Backend.Services.Implementations
{
    /// <summary>
    /// Provides token management services, including creation and validation of JWT access tokens and refresh tokens.
    /// </summary>
    public sealed class TokenService : ITokenService
    {
        private readonly RefreshTokenConfig _refreshTokenConfig;
        private readonly JwtTokenConfig _jwtTokenConfig;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITwoFactorAuthService _twoFactorAuthService;

        /// <summary>       
        /// Initializes a new instance of <see cref="TokenService"/> with required configurations and dependencies.
        /// </summary>
        /// <param name="refreshTokenConfig">Configuration for refresh token lifespan and settings.</param>
        /// <param name="jwtTokenConfig">Configuration for JWT signing, issuer, audience, and expiration.</param>
        /// <param name="refreshTokenRepository">Repository for managing refresh token persistence.</param>
        /// <param name="databaseContext">The database context used to execute SQL queries.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is <c>null</c>.</exception>
        public TokenService(
            IOptions<RefreshTokenConfig> refreshTokenConfig,
            IOptions<JwtTokenConfig> jwtTokenConfig,
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            ITwoFactorAuthService twoFactorAuthService)
        {
            _refreshTokenConfig = refreshTokenConfig.Value
                ?? throw new ArgumentNullException(nameof(refreshTokenConfig));

            _jwtTokenConfig = jwtTokenConfig.Value
                ?? throw new ArgumentNullException(nameof(jwtTokenConfig));

            _refreshTokenRepository = refreshTokenRepository
                ?? throw new ArgumentNullException(nameof(refreshTokenRepository));

            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));

            _twoFactorAuthService = twoFactorAuthService
                ?? throw new ArgumentNullException(nameof(twoFactorAuthService));
        }

        /// <inheritdoc/>
        public async Task<AuthRefreshResponse> RefreshAccessTokenAsync(AuthRefreshRequest request, DeviceContext deviceContext, CancellationToken cancellationToken = default)
        {
            string tokenHash =
                Sha256Hasher.ComputeBase64(Convert.FromBase64String(request.RefreshToken));

            var tokenEntity =
                await _refreshTokenRepository.GetByTokenAsync(tokenHash, cancellationToken);

            if (tokenEntity is null ||
                tokenEntity.TokenHash != tokenHash ||
                !tokenEntity.IsActive)
            {
                throw new AuthenticationRequiredException();
            }

            if (deviceContext.HardwareFingerprint != tokenEntity.HardwareFingerprint)
            {
                throw new InvalidClientContextException();
            }

            var accessToken =
                await CreateAccessTokenAsync(tokenEntity.UserId, cancellationToken);

            return new AuthRefreshResponse
            {
                AccessToken = accessToken
            };
        }

        /// <inheritdoc/>
        public async Task<RefreshTokenDto> CreateRefreshTokenAsync(uint userId, DeviceContext deviceContext, CancellationToken cancellationToken = default)
        {
            byte[] rawToken =
                CryptographyUtilities.GenerateSecureRandomBytes(32);

            string tokenHash =
                Sha256Hasher.ComputeBase64(rawToken);

            var now = DateTime.UtcNow;
            var expiration = now.Add(_refreshTokenConfig.Expiration);

            var refreshTokenEntity = new RefreshTokenEntity
            {
                TokenHash = tokenHash,
                UserId = userId,
                IpAddress = deviceContext.IpAddress,
                DeviceIdentifier = deviceContext.DeviceIdentifier,
                DeviceOs = deviceContext.DeviceOs,
                HardwareFingerprint = deviceContext.HardwareFingerprint,
                NetworkFingerprint = deviceContext.NetworkFingerprint,
                IssuedAt = now,
                ExpiresAt = expiration,
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateOrUpdateAsync(refreshTokenEntity, cancellationToken);

            return new RefreshTokenDto
            {
                Token = Convert.ToBase64String(rawToken),
                ExpiresAt = expiration
            };
        }

        /// <inheritdoc/>
        public async Task<AccessTokenDto> CreateAccessTokenAsync(uint userId, CancellationToken cancellationToken = default)
        {
            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user is null)
            {
                throw new UserNotFoundException();
            }

            byte[] key =
                Convert.FromBase64String(_jwtTokenConfig.SigningKey);

            var tokenExpiry =
                DateTime.UtcNow.Add(_jwtTokenConfig.Expiration);

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
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token =
                tokenHandler.CreateToken(tokenDescriptor);

            return new AccessTokenDto
            {
                Token = tokenHandler.WriteToken(token),
                ExpiresAt = tokenExpiry
            };
        }

        /// <inheritdoc/>
        public async Task RevokeSessionAsync(uint userId, SessionRevokeRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _twoFactorAuthService.VerifyAsync(
                    userId,
                    new TwoFactorVerificationRequest
                    {
                        TwoFactorCode = request.TwoFactorCode
                    },
                    cancellationToken);
            }
            catch (TwoFactorNotConfiguredException) { }

            var token =
                await _refreshTokenRepository.GetByTokenAsync(request.TokenHash, cancellationToken);

            if (token is null || token.UserId != userId)
            {
                throw new AccessProhibitedException();
            }

            await _refreshTokenRepository.RevokeAsync(request.TokenHash, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RevokeAllSessionsAsync(uint userId, TwoFactorVerificationRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _twoFactorAuthService.VerifyAsync(userId, request, cancellationToken);
            }
            catch (TwoFactorNotConfiguredException) { }
            

            await _refreshTokenRepository.RevokeAllForUserAsync(userId, cancellationToken);
        }
    }
}
