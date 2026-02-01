using BlockSense.Backend.Data;
using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Models.DeviceContext;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Cryptography.Hashing;
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
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
        private readonly ITokenService _tokenService;
        private readonly ITwoFactorAuthService _twoFactorAuthService;
        private readonly DatabaseContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of <see cref="AuthService"/> with required dependencies.
        /// </summary>
        /// <param name="userRepository">The repository for user entity operations.</param>
        /// <param name="tokenService">The service responsible for generating access and refresh tokens.</param>
        /// <param name="databaseContext">The database context used to execute SQL queries.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is <c>null</c>.</exception>
        public AuthService(
            IUserRepository userRepository,
            ITwoFactorAuthRepository twoFactorAuthRepository,
            ITokenService tokenService,
            ITwoFactorAuthService twoFactorAuthService,
            DatabaseContext databaseContext)
        {
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));

            _twoFactorAuthRepository = twoFactorAuthRepository
                ?? throw new ArgumentNullException(nameof(twoFactorAuthRepository));

            _tokenService = tokenService
                ?? throw new ArgumentNullException(nameof(tokenService));

            _twoFactorAuthService = twoFactorAuthService
                ?? throw new ArgumentNullException(nameof(twoFactorAuthService));

            _databaseContext = databaseContext
                ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <inheritdoc/>
        public async Task<AuthResponse> AuthenticateAsync(AuthRequest request, DeviceContext deviceContext, CancellationToken cancellationToken = default)
        {
            request = request with
            {
                Login = request.Login.Trim(),
                Password = request.Password.Trim()
            };

            await _databaseContext.BeginTransactionAsync(cancellationToken: cancellationToken);

            try
            {
                var user =
                    await _userRepository.GetByUsernameOrEmailAsync(request.Login, cancellationToken);

                if (user is null || user.IsDeleted)
                {
                    throw new InvalidCredentialsException();
                }

                if (user.UserType is UserType.Banned)
                {
                    throw new AccessProhibitedException();
                }

                var argon2idHasher = new Argon2idHasher();

                var computedHash = argon2idHasher.Derive(
                    Encoding.UTF8.GetBytes(request.Password),
                    out _,
                    user.PasswordSalt);

                if (!Arrays.FixedTimeEquals(user.PasswordHash, computedHash))
                {
                    throw new InvalidCredentialsException();
                }

                if (await _twoFactorAuthRepository.IsEnabledAsync(user.UserId, cancellationToken))
                {
                    if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
                    {
                        throw new TwoFactorRequiredException();
                    }

                    await _twoFactorAuthService.VerifyAsync(user.UserId, new TwoFactorVerificationRequest
                    {
                        TwoFactorCode = request.TwoFactorCode
                    });
                }

                var accessToken =
                    await _tokenService.CreateAccessTokenAsync(user.UserId, cancellationToken);

                var refreshToken =
                    await _tokenService.CreateRefreshTokenAsync(
                        user.UserId,
                        deviceContext,
                        cancellationToken);

                await _databaseContext.CommitAsync(cancellationToken);

                return new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }
            catch
            {
                await _databaseContext.RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                await _databaseContext.DisposeAsync();
            }
        }
    }
}
