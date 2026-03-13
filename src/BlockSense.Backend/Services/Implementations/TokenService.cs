using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Exceptions.Generic;
using BlockSense.Backend.Exceptions.TwoFactorAuthentication;
using BlockSense.Backend.Models.ActivityLog;
using BlockSense.Backend.Models.Device;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.Cryptography.Utilities;
using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.Token;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using BlockSense.Contracts.Enums;
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
        private readonly IActivityLogService _activityLogService;

        /// <summary>
        /// Initializes a new instance of <see cref="TokenService"/> with required configurations and dependencies.
        /// </summary>
        /// <param name="refreshTokenConfig">Configuration for refresh token lifespan and settings.</param>
        /// <param name="jwtTokenConfig">Configuration for JWT signing, issuer, audience, and expiration.</param>
        /// <param name="refreshTokenRepository">Repository for managing refresh token persistence.</param>
        /// <param name="userRepository">Repository for user entity operations.</param>
        /// <param name="twoFactorAuthService">The service responsible for two-factor authentication operations.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is <c>null</c>.</exception>
        public TokenService(
            IOptions<RefreshTokenConfig> refreshTokenConfig,
            IOptions<JwtTokenConfig> jwtTokenConfig,
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            ITwoFactorAuthService twoFactorAuthService,
            IActivityLogService activityLogService)
        {
            _refreshTokenConfig = refreshTokenConfig.Value ?? throw new ArgumentNullException(nameof(refreshTokenConfig));
            _jwtTokenConfig = jwtTokenConfig.Value ?? throw new ArgumentNullException(nameof(jwtTokenConfig));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _twoFactorAuthService = twoFactorAuthService ?? throw new ArgumentNullException(nameof(twoFactorAuthService));
            _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
        }

        /// <inheritdoc/>
        public async Task<AuthRefreshResponse> RefreshAccessTokenAsync(
            AuthRefreshRequest request,
            DeviceContext deviceContext,
            CancellationToken cancellationToken = default)
        {
            var tokenEntity = await GetValidatedRefreshTokenAsync(request.RefreshToken, cancellationToken);

            EnsureDeviceMatch(deviceContext, tokenEntity);

            var accessToken = await CreateAccessTokenAsync(tokenEntity.UserId, cancellationToken);

            return new AuthRefreshResponse
            {
                AccessToken = accessToken
            };
        }

        /// <inheritdoc/>
        public async Task<RefreshTokenDto> CreateRefreshTokenAsync(
            uint userId,
            DeviceContext deviceContext,
            CancellationToken cancellationToken = default)
        {
            var rawToken = CryptographyUtilities.GenerateSecureRandomBytes(32);
            var tokenHash = Sha256Hasher.ComputeBase64(rawToken);
            var now = DateTime.UtcNow;
            var expiry = now.Add(_refreshTokenConfig.Expiration);

            var tokenEntity = BuildRefreshTokenEntity(userId, tokenHash, deviceContext, now, expiry);

            await _refreshTokenRepository.CreateAsync(tokenEntity, cancellationToken);

            return new RefreshTokenDto
            {
                Token = Convert.ToBase64String(rawToken),
                ExpiresAt = expiry
            };
        }

        /// <inheritdoc/>
        public async Task<AccessTokenDto> CreateAccessTokenAsync(uint userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user is null)
                throw new NotFoundException();

            var expiry = DateTime.UtcNow.Add(_jwtTokenConfig.Expiration);
            var tokenDescriptor = BuildTokenDescriptor(user, expiry);
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.CreateToken(tokenDescriptor);

            return new AccessTokenDto
            {
                Token = tokenHandler.WriteToken(accessToken),
                ExpiresAt = expiry
            };
        }

        /// <inheritdoc/>
        public async Task RevokeSessionAsync(uint userId, SessionRevokeRequest request, CancellationToken cancellationToken = default)
        {
            await VerifyTwoFactorIfRequiredAsync(userId, request.TwoFactorCode, cancellationToken);

            var token = await _refreshTokenRepository.GetByTokenHashAsync(request.TokenHash, cancellationToken);

            if (token is null || token.UserId != userId)
                throw new NotFoundException();

            await _refreshTokenRepository.RevokeAsync(request.TokenHash, cancellationToken);

            await LogDeviceRevokedAsync(userId, token, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RevokeAllSessionsAsync(uint userId, RevokeAllSessionsRequest request, CancellationToken cancellationToken = default)
        {
            await VerifyTwoFactorIfRequiredAsync(userId, request.TwoFactorCode, cancellationToken);

            await _refreshTokenRepository.RevokeAllByUserIdAsync(userId, cancellationToken);
        }

        private async Task<RefreshToken> GetValidatedRefreshTokenAsync(string rawBase64Token, CancellationToken cancellationToken)
        {
            var tokenHash = Sha256Hasher.ComputeBase64(Convert.FromBase64String(rawBase64Token));
            var tokenEntity = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

            if (tokenEntity is null || !tokenEntity.IsValid)
                throw new AuthenticationRequiredException();

            return tokenEntity;
        }

        private static void EnsureDeviceMatch(DeviceContext deviceContext, RefreshToken tokenEntity)
        {
            if (deviceContext.HardwareFingerprint != tokenEntity.HardwareFingerprint)
                throw new InvalidClientContextException();
        }

        private static RefreshToken BuildRefreshTokenEntity(
            uint userId,
            string tokenHash,
            DeviceContext deviceContext,
            DateTime issuedAt,
            DateTime expiresAt) => new()
            {
                TokenHash = tokenHash,
                UserId = userId,
                IpAddress = deviceContext.IpAddress,
                DeviceIdentifier = deviceContext.DeviceIdentifier,
                DeviceOs = deviceContext.DeviceOs,
                HardwareFingerprint = deviceContext.HardwareFingerprint,
                NetworkFingerprint = deviceContext.NetworkFingerprint,
                IssuedAt = issuedAt,
                ExpiresAt = expiresAt,
                IsRevoked = false
            };

        private SecurityTokenDescriptor BuildTokenDescriptor(User user, DateTime expiry)
        {
            var signingKey = Convert.FromBase64String(_jwtTokenConfig.SigningKey);

            return new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Typ, user.Role.ToString())
                }),
                Expires = expiry,
                Issuer = _jwtTokenConfig.Issuer,
                Audience = _jwtTokenConfig.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(signingKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };
        }

        private async Task VerifyTwoFactorIfRequiredAsync(
            uint userId,
            string? code,
            CancellationToken cancellationToken)
        {
            try
            {
                await _twoFactorAuthService.VerifyAsync(
                    userId,
                    new TwoFactorVerificationRequest { TwoFactorCode = code ?? string.Empty },
                    cancellationToken);
            }
            catch (TwoFactorConfigurationException)
            {
                // 2FA is not enabled for this user — no verification required.
            }
            catch (TwoFactorInvalidCodeException) when (string.IsNullOrWhiteSpace(code))
            {
                throw new TwoFactorRequiredException();
            }
        }

        private Task LogDeviceRevokedAsync(
            uint userId,
            RefreshToken token,
            CancellationToken cancellationToken)
        {
            var context = new ActivityLogContext()
                .WithIpAddress(token.IpAddress);

            return _activityLogService.CreateAsync(
                ActivityType.User,
                userId,
                ActivityActions.Device.Revoked,
                context,
                cancellationToken);
        }
    }
}