using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Exceptions.Registration;
using BlockSense.Backend.Exceptions.User;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Contracts.Enums.User;
using MySql.Data.MySqlClient;
using System.Text;

namespace BlockSense.Backend.Services.Implementations
{
    /// <summary>
    /// Implements user-related operations, including account registration, with transactional safety and invitation validation.
    /// </summary>
    public sealed class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITwoFactorAuthRepository _twoFactorAuthRepository;
        private readonly DatabaseContext _databaseContext;

        /// <summary>
        /// Initializes a new instance of <see cref="UserService"/> with required dependencies.
        /// </summary>
        /// <param name="userRepository">The repository for user entity operations.</param>
        /// <param name="invitationRepository">The repository for invitation entity operations.</param>
        /// <param name="databaseContext">The database context used to execute SQL queries.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is <c>null</c>.</exception>
        public UserService(
            IUserRepository userRepository,
            IInvitationRepository invitationRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITwoFactorAuthRepository twoFactorAuthRepository,
            DatabaseContext databaseContext)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _twoFactorAuthRepository = twoFactorAuthRepository ?? throw new ArgumentNullException(nameof(twoFactorAuthRepository));
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        /// <inheritdoc/>
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

                if (invitation is null)
                {
                    throw new InvalidInvitationCodeException();
                }

                var argon2idHasher = new Argon2idHasher();
                var computedHash = argon2idHasher.Derive(
                    Encoding.UTF8.GetBytes(request.Password),
                    out byte[] computedSalt);

                var now = DateTime.UtcNow;

                var user = new UserEntity
                {
                    UserId = uint.MinValue,
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = computedHash,
                    PasswordSalt = computedSalt,
                    UserType = UserType.Standard,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                uint userId =
                    await _userRepository.CreateAsync(user, cancellationToken);

                await _invitationRepository.MarkAsUsedAsync(
                    invitation.InvitationId,
                    userId,
                    cancellationToken);

                await _databaseContext.CommitAsync(cancellationToken);

                return new RegistrationResponse
                {
                    UserId = userId,
                    Username = user.Username,
                    Email = user.Email,
                    UserType = user.UserType,
                    CreatedAt = user.CreatedAt
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

        public async Task<UserSummaryDto> GetUserSummaryAsync(uint userId, CancellationToken cancellationToken = default)
        {
            var user =
                await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                throw new UserNotFoundException();
            }

            var invitedBy =
                await _invitationRepository.GetUsernameByUsedUser(userId, cancellationToken);

            var twoFaEnabled =
                await _twoFactorAuthRepository.IsEnabledAsync(userId);

            return new UserSummaryDto
            {
                UserId = userId,
                Username = user.Username,
                Email = user.Email,
                UserType = user.UserType,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                InvitedBy = invitedBy ?? "Unknown",
                TwoFactorEnabled = twoFaEnabled
            };
        }

        public async Task<UserDashboardDto> GetUserDashboardAsync(uint userId, CancellationToken cancellationToken = default)
        {
            var userSummary =
                await GetUserSummaryAsync(userId, cancellationToken);

            var activeTokens =
                await _refreshTokenRepository.GetActiveByUserAsync(userId, cancellationToken);

            var invitationCodes =
                await _invitationRepository.GetByUserAsync(userId, cancellationToken);

            return new UserDashboardDto
            {
                Profile = userSummary,
                ActiveTokens = ToActiveUserTokens(activeTokens),
                UserInvitations = ToUserInvitations(invitationCodes)
            };
        }

        private static IReadOnlyList<UserTokenSessionDto> ToActiveUserTokens(IReadOnlyList<RefreshTokenEntity> tokens)
        {
            return tokens
                .Select(token => new UserTokenSessionDto
                {
                    TokenHash = token.TokenHash,
                    DeviceName = token.DeviceIdentifier,
                    IpAddress = token.IpAddress,
                    IssuedAt = token.IssuedAt,
                    ExpiresAt = token.ExpiresAt
                })
                .ToList();
        }

        private static IReadOnlyList<InvitationCodeDto> ToUserInvitations(IReadOnlyList<InvitationCodeEntity> invitations)
        {
            return invitations
                .Select(invitation => new InvitationCodeDto
                {
                    InvitationCode = invitation.InvitationCode,
                    CreatedAt = invitation.CreatedAt,
                    ExpiresAt = invitation.ExpiresAt,
                    InvitedUser = invitation.UsedByUsername,
                    Status = invitation.Status
                })
                .ToList();
        }

    }
}
