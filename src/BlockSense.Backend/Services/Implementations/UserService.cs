using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Exceptions.Generic;
using BlockSense.Backend.Exceptions.Registration;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Backend.Utilities;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.DTOs.Invitation;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Contracts.DTOs.Session;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Contracts.Enums;
using MySql.Data.MySqlClient;
using System.Text;

namespace BlockSense.Backend.Services.Implementations
{
    /// <summary>
    /// Implements user-related operations, including account registration, profile retrieval, and dashboard data aggregation.
    /// </summary>
    public sealed class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IInvitationRepository _invitationRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITotpCredentialRepository _totpCredentialRepository;
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly DatabaseContext _databaseContext;
        private readonly Argon2idHasher _argon2IdHasher;

        /// <summary>
        /// Initializes a new instance of <see cref="UserService"/> with required dependencies.
        /// </summary>
        /// <param name="userRepository">The repository for user entity operations.</param>
        /// <param name="invitationRepository">The repository for invitation entity operations.</param>
        /// <param name="refreshTokenRepository">The repository for refresh token entity operations.</param>
        /// <param name="totpCredentialRepository">The repository for TOTP credential entity operations.</param>
        /// <param name="activityLogRepository">The repository for activity log entity operations.</param>
        /// <param name="databaseContext">The database context used to manage transactions.</param>
        /// <exception cref="ArgumentNullException">Thrown if any dependency is <c>null</c>.</exception>
        public UserService(
            IUserRepository userRepository,
            IInvitationRepository invitationRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITotpCredentialRepository totpCredentialRepository,
            IActivityLogRepository activityLogRepository,
            DatabaseContext databaseContext)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _totpCredentialRepository = totpCredentialRepository ?? throw new ArgumentNullException(nameof(totpCredentialRepository));
            _activityLogRepository = activityLogRepository ?? throw new ArgumentNullException(nameof(activityLogRepository));
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
            _argon2IdHasher = new Argon2idHasher();
        }

        /// <inheritdoc/>
        public async Task<RegistrationResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
        {
            var normalizedRequest = NormalizeRegistrationRequest(request);

            await _databaseContext.BeginTransactionAsync(cancellationToken: cancellationToken);

            try
            {
                var invitation = await GetValidInvitationAsync(normalizedRequest.InvitationCode, cancellationToken);
                var user = BuildNewUser(normalizedRequest);

                uint userId = await _userRepository.CreateAsync(user, cancellationToken);

                await _invitationRepository.RedeemAsync(invitation.Id, userId, cancellationToken);
                await _databaseContext.CommitTransactionAsync(cancellationToken);

                return BuildRegistrationResponse(userId, user);
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                await _databaseContext.RollbackTransactionAsync(cancellationToken);
                throw ResolveUniqueConstraintException(ex);
            }
            catch
            {
                await _databaseContext.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<UserSummaryDto> GetUserSummaryAsync(uint userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException();

            var invitedBy = await _userRepository.GetInviterUsernameAsync(userId, cancellationToken) ?? "Unknown";
            var twoFactorEnabled = await _totpCredentialRepository.ExistsAsync(userId, cancellationToken);

            return new UserSummaryDto
            {
                UserId = userId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                InvitedBy = invitedBy,
                TwoFactorEnabled = twoFactorEnabled
            };
        }

        /// <inheritdoc/>
        public async Task<UserDashboardDto> GetUserDashboardAsync(uint userId, CancellationToken cancellationToken = default)
        {
            var profile = await GetUserSummaryAsync(userId, cancellationToken);
            var activeSessions = await _refreshTokenRepository.GetActiveByUserIdAsync(userId, cancellationToken);
            var recentActivity = await _activityLogRepository.GetPagedByUserIdAsync(userId, page: 1, pageSize: 3, cancellationToken);
            var invitations = await _invitationRepository.GetByIssuedToIdAsync(userId, cancellationToken);

            return new UserDashboardDto
            {
                Profile = profile,
                ActiveTokens = activeSessions.Select(MapToSessionDto).ToList().AsReadOnly(),
                RecentActivity = recentActivity.Select(MapToActivityLogDto).ToList().AsReadOnly(),
                UserInvitations = invitations.Select(MapToInvitationDto).ToList().AsReadOnly()
            };
        }

        private static RegistrationRequest NormalizeRegistrationRequest(RegistrationRequest request) =>
            request with
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                InvitationCode = request.InvitationCode.Trim()
            };

        private async Task<InvitationCode> GetValidInvitationAsync(string code, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByCodeForUpdateAsync(code, cancellationToken);

            if (invitation is null || !invitation.IsValid)
                throw new InvalidInvitationCodeException();

            return invitation;
        }

        private User BuildNewUser(RegistrationRequest request)
        {
            var hash = _argon2IdHasher.Derive(Encoding.UTF8.GetBytes(request.Password), out byte[] salt);
            var now = DateTime.UtcNow;

            return new User
            {
                Id = default,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = UserRole.Standard,
                CreatedAt = now,
                UpdatedAt = now,
                DeletedAt = null
            };
        }

        private static RegistrationResponse BuildRegistrationResponse(uint userId, User user) => new()
        {
            UserId = userId,
            Username = user.Username,
            Email = user.Email,
            UserRole = user.Role,
            CreatedAt = user.CreatedAt
        };

        private static Exception ResolveUniqueConstraintException(MySqlException ex)
        {
            if (ex.Message.Contains("uq_users_username"))
                return new UsernameTakenException();

            if (ex.Message.Contains("uq_users_email"))
                return new EmailTakenException();

            return ex;
        }

        private static SessionDto MapToSessionDto(RefreshToken token) => new()
        {
            TokenHash = token.TokenHash,
            IpAddress = IpAddressMasker.Mask(token.IpAddress),
            IssuedAt = token.IssuedAt,
            ExpiresAt = token.ExpiresAt
        };

        private static InvitationDto MapToInvitationDto(InvitationCode invitation) => new()
        {
            Code = invitation.Code,
            RedeemedBy = invitation.RedeemedByUsername,
            CreatedAt = invitation.CreatedAt,
            ExpiresAt = invitation.ExpiresAt,
            Status = ResolveInvitationStatus(invitation),
            IsRevoked = invitation.IsRevoked
        };

        private static ActivityLogDto MapToActivityLogDto(ActivityLog log) => new()
        {
            Id = log.Id,
            Type = log.Type,
            UserId = log.UserId,
            Action = log.Action,
            ActivityMessage = ActivityMessageMapper.Map(log.Action, log.Context),
            OccurredAt = log.OccurredAt
        };

        private static InvitationStatus ResolveInvitationStatus(InvitationCode invitation)
        {
            if (invitation.IsRevoked) return InvitationStatus.Revoked;
            if (invitation.IsRedeemed) return InvitationStatus.Used;
            if (DateTime.UtcNow >= invitation.ExpiresAt) return InvitationStatus.Expired;

            return InvitationStatus.Active;
        }
    }
}