using BlockSense.Backend.Data;
using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Exceptions.Generic;
using BlockSense.Backend.Models.ActivityLog;
using BlockSense.Backend.Models.Device;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using BlockSense.Contracts.Enums;
using Org.BouncyCastle.Utilities;
using System.Text;

namespace BlockSense.Backend.Services.Implementations
{
    /// <summary>
    /// Provides authentication services, including credential validation, token issuance, and device-based session tracking.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITotpCredentialRepository _totpCredentialRepository;
        private readonly ITokenService _tokenService;
        private readonly ITwoFactorAuthService _twoFactorAuthService;
        private readonly IActivityLogService _activityLogService;
        private readonly DatabaseContext _databaseContext;
        private readonly Argon2idHasher _argon2IdHasher;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthService"/> with required dependencies.
        /// </summary>
        /// <param name="userRepository">The repository for user entity operations.</param>
        /// <param name="totpCredentialRepository">The repository for TOTP credential operations.</param>
        /// <param name="tokenService">The service responsible for generating access and refresh tokens.</param>
        /// <param name="twoFactorAuthService">The service responsible for two-factor authentication operations.</param>
        /// <param name="activityLogService">The service responsible for recording user and system activity logs.</param>
        /// <param name="databaseContext">The database context used to manage transactions.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is <c>null</c>.</exception>
        public AuthService(
            IUserRepository userRepository,
            ITotpCredentialRepository totpCredentialRepository,
            ITokenService tokenService,
            ITwoFactorAuthService twoFactorAuthService,
            IActivityLogService activityLogService,
            DatabaseContext databaseContext)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _totpCredentialRepository = totpCredentialRepository ?? throw new ArgumentNullException(nameof(totpCredentialRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _twoFactorAuthService = twoFactorAuthService ?? throw new ArgumentNullException(nameof(twoFactorAuthService));
            _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
            _argon2IdHasher = new Argon2idHasher();
        }

        /// <inheritdoc/>
        public async Task<AuthResponse> AuthenticateAsync(
            AuthRequest request,
            DeviceContext deviceContext,
            CancellationToken cancellationToken = default)
        {
            var normalizedRequest = NormalizeRequest(request);

            await _databaseContext.BeginTransactionAsync(cancellationToken: cancellationToken);

            try
            {
                var user = await GetValidUserAsync(normalizedRequest.Login, cancellationToken);

                VerifyPassword(normalizedRequest.Password, user);

                await VerifyTwoFactorIfEnabledAsync(user.Id, normalizedRequest.TwoFactorCode, cancellationToken);

                var accessToken = await _tokenService.CreateAccessTokenAsync(user.Id, cancellationToken);
                var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id, deviceContext, cancellationToken);

                await _databaseContext.CommitTransactionAsync(cancellationToken);

                await LogAuthenticationAsync(user.Id, deviceContext, cancellationToken);

                return new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }
            catch
            {
                await _databaseContext.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        private static AuthRequest NormalizeRequest(AuthRequest request) =>
            request with
            {
                Login = request.Login.Trim(),
                Password = request.Password.Trim()
            };

        private async Task<Entities.User> GetValidUserAsync(string login, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(login, cancellationToken);

            if (user is null || user.IsDeleted)
                throw new InvalidCredentialsException();

            if (user.Role is UserRole.Banned)
                throw new ForbiddenException();

            return user;
        }

        private void VerifyPassword(string plainPassword, Entities.User user)
        {
            var computedHash = _argon2IdHasher.Derive(
                Encoding.UTF8.GetBytes(plainPassword),
                out _,
                user.PasswordSalt);

            if (!Arrays.FixedTimeEquals(user.PasswordHash, computedHash))
                throw new InvalidCredentialsException();
        }

        private async Task VerifyTwoFactorIfEnabledAsync(
            uint userId,
            string? code,
            CancellationToken cancellationToken)
        {
            if (!await _totpCredentialRepository.ExistsAsync(userId, cancellationToken))
                return;

            if (string.IsNullOrWhiteSpace(code))
                throw new TwoFactorRequiredException();

            await _twoFactorAuthService.VerifyAsync(
                userId,
                new TwoFactorVerificationRequest { TwoFactorCode = code },
                cancellationToken);
        }

        private Task LogAuthenticationAsync(
            uint userId,
            DeviceContext deviceContext,
            CancellationToken cancellationToken)
        {
            var context = new ActivityLogContext()
                .WithIpAddress(deviceContext.IpAddress);

            return _activityLogService.CreateAsync(
                ActivityType.User,
                userId,
                ActivityActions.Device.Authenticated,
                context,
                cancellationToken);
        }
    }
}