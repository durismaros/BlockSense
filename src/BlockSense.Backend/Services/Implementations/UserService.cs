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
using System.Net;
using System.Net.Sockets;
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
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));

            _invitationRepository = invitationRepository
                ?? throw new ArgumentNullException(nameof(invitationRepository));

            _refreshTokenRepository = refreshTokenRepository
                ?? throw new ArgumentNullException(nameof(refreshTokenRepository));

            _twoFactorAuthRepository = twoFactorAuthRepository
                ?? throw new ArgumentNullException(nameof(twoFactorAuthRepository));

            _databaseContext = databaseContext
                ?? throw new ArgumentNullException(nameof(databaseContext));
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
                        cancellationToken) ?? throw new InvalidInvitationCodeException();

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
                await _userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new UserNotFoundException();

            var invitedBy =
                await _invitationRepository.GetInviterUsernameByUser(userId, cancellationToken) ?? "Unknown";

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
                InvitedBy = invitedBy,
                TwoFactorEnabled = twoFaEnabled
            };
        }

        public async Task<UserDashboardDto> GetUserDashboardAsync(uint userId, CancellationToken cancellationToken = default)
        {
            var userSummary =
                await GetUserSummaryAsync(userId, cancellationToken);

            var activeTokens =
                await _refreshTokenRepository.GetActiveSessionsByUserAsync(userId, cancellationToken);

            var invitationCodes =
                await _invitationRepository.GetDtoByUserAsync(userId, cancellationToken);

            return new UserDashboardDto
            {
                Profile = userSummary,
                ActiveTokens = activeTokens.Select(token => token with
                {
                    IpAddress = MaskIp(token.IpAddress)
                })
                .ToList(),
                UserInvitations = invitationCodes
            };
        }

        private static string MaskIp(string ipString)
        {
            if (!IPAddress.TryParse(ipString, out var ip))
            {
                return ipString;
            }

            // Normalize IPv4-mapped IPv6
            if (ip.IsIPv4MappedToIPv6)
            {
                ip = ip.MapToIPv4();
            }

            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                var parts = ip.ToString().Split('.');
                return $"{parts[0]}.{parts[1]}.{parts[2]}.*";
            }

            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var full = ip.ToString();

                var hextets = full.Split(':');

                // Keep first 4 hextets, mask the rest
                var visible = hextets.Take(4).ToList();

                while (visible.Count < 4)
                {
                    visible.Add("0");
                }

                return string.Join(':', visible) + ":*:*:*:*";
            }

            return ipString;
        }
    }
}
