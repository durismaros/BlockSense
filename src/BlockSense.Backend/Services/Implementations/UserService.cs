using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Exceptions.Registration;
using BlockSense.Backend.Models;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.DTOs.Auth.Login;
using BlockSense.Contracts.DTOs.Auth.Register;
using BlockSense.Contracts.Enums.User;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
using System.Text;

namespace BlockSense.Backend.Services.Implementations
{
    public sealed class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IInvitationRepository _invitationRepository;
        private readonly ITokenService _tokenService;
        private readonly DatabaseContext _databaseContext;

        public UserService(
            IUserRepository userRepository,
            IInvitationRepository invitationRepository,
            ITokenService tokenService,
            DatabaseContext databaseContext)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        public async Task<RegistrationResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
        {
            request = request with
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                InvitationCode = request.InvitationCode.Trim()
            };

            await _databaseContext.BeginTransactionAsync(cancellationToken: cancellationToken);

            try
            {
                var invitation =
                    await _invitationRepository.GetByCodeForUpdateAsync(
                        request.InvitationCode,
                        cancellationToken);

                if (invitation == null ||
                    invitation.IsUsed ||
                    invitation.IsRevoked ||
                    invitation.ExpiresAt < DateTime.UtcNow)
                {
                    throw new InvalidInvitationCodeException();
                }

                var argon2idHasher = new Argon2idHasher();
                var computedHash = argon2idHasher.Derive(
                    Encoding.UTF8.GetBytes(request.Password),
                    out byte[] computedSalt);

                var now = DateTime.UtcNow;

                var userEntity = new UserEntity
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = computedHash,
                    PasswordSalt = computedSalt,
                    UserType = UserType.Standard,
                    InvitationCodeId = invitation.InvitationCodeId,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                uint userId =
                    await _userRepository.CreateAsync(userEntity, cancellationToken);

                await _invitationRepository.MarkAsUsedAsync(
                    invitation.InvitationCodeId,
                    cancellationToken);

                await _databaseContext.CommitAsync(cancellationToken);

                return new RegistrationResponse
                {
                    UserId = userId,
                    Username = userEntity.Username,
                    Email = userEntity.Email,
                    UserType = userEntity.UserType,
                    CreatedAt = userEntity.CreatedAt
                };
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                await _databaseContext.RollbackAsync(cancellationToken);

                if (ex.Message.Contains("uq_users_username"))
                {
                    throw new UsernameTakenException();
                }

                if (ex.Message.Contains("uq_users_email"))
                {
                    throw new EmailTakenException();
                }

                throw;
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

        public async Task<LoginResponse> LoginAsync(LoginRequest request, DeviceContext deviceContext, CancellationToken cancellationToken = default)
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

                if (user == null || user.DeletedAt.HasValue)
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

                return new LoginResponse
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
