using BlockSense.Backend.Data;
using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Models;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.DTOs.Auth;
using BlockSense.Contracts.Enums.User;
using Org.BouncyCastle.Utilities;
using System.Text;

namespace BlockSense.Backend.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly DatabaseContext _databaseContext;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            DatabaseContext databaseContext)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

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
                var user = await _userRepository.GetByUsernameOrEmailAsync(
                    request.Login,
                    cancellationToken);

                if (user is null || user.DeletedAt.HasValue)
                {
                    throw new InvalidCredentialsException();
                }

                if (user.UserType == UserType.Banned)
                {
                    throw new AccountBannedException();
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

                var accessToken =
                    await _tokenService.CreateAccessTokenAsync(user, cancellationToken);

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
