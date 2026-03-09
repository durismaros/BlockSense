using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Exceptions.Generic;
using BlockSense.Backend.Exceptions.Registration;
using BlockSense.Backend.Repositories.Implementations;
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
        private readonly ITotpCredentialRepository _totpCredentialRepository;
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly DatabaseContext _databaseContext;
        private readonly Argon2idHasher _argon2IdHasher;

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
            _argon2IdHasher = new Argon2idHasher() ?? throw new ArgumentNullException(nameof(Argon2idHasher));
        }

        /// <inheritdoc/>
        public async Task<RegistrationResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
        {
            request = request with
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                InvitationCode = request.InvitationCode.Trim(),
            };

            await _databaseContext.BeginTransactionAsync(cancellationToken: cancellationToken);

            try
            {
                var invitation =
                    await _invitationRepository.GetByCodeForUpdateAsync(request.InvitationCode, cancellationToken);

                if (invitation is null || !invitation.IsValid)
                {
                    throw new InvalidInvitationCodeException();
                }

                var user = BuildNewUser(request);

                uint userId =
                    await _userRepository.CreateAsync(user, cancellationToken);

                await _invitationRepository.RedeemAsync(
                    invitation.Id,
                    userId,
                    cancellationToken);

                await _databaseContext.CommitTransactionAsync(cancellationToken);

                return new RegistrationResponse
                {
                    UserId = userId,
                    Username = user.Username,
                    Email = user.Email,
                    UserRole = user.Role,
                    CreatedAt = user.CreatedAt
                };
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                await _databaseContext.RollbackTransactionAsync(cancellationToken);

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
                await _databaseContext.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<UserSummaryDto> GetUserSummaryAsync(uint userId, CancellationToken cancellationToken = default)
        {
            var user =
                await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user is null)
            {
                throw new NotFoundException();
            }

            var invitedBy =
                await _userRepository.GetInviterUsernameAsync(userId, cancellationToken)
                ?? "Unknown";

            var twoFaEnabled =
                await _totpCredentialRepository.ExistsAsync(userId);

            return new UserSummaryDto
            {
                UserId = userId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
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
                await _refreshTokenRepository.GetActiveByUserIdAsync(userId, cancellationToken);

            var recentActivity =
                await _activityLogRepository.GetPagedByUserIdAsync(userId, page: 1, pageSize: 3, cancellationToken);

            var invitationCodes =
                await _invitationRepository.GetByIssuedToIdAsync(userId, cancellationToken);

            return new UserDashboardDto
            {
                Profile = userSummary,

                ActiveTokens = activeTokens
                    .Select(MapToSessionDto)
                    .ToList()
                    .AsReadOnly(),

                RecentActivity = recentActivity
                    .Select(MapToActivityLogDto)
                    .ToList()
                    .AsReadOnly(),
                
                UserInvitations = invitationCodes
                    .Select(MapToInvitationDto)
                    .ToList()
                    .AsReadOnly(),
            };
        }

        private User BuildNewUser(RegistrationRequest request)
        {
            var hash = _argon2IdHasher.Derive(
                Encoding.UTF8.GetBytes(request.Password),
                out byte[] salt);

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

        private static SessionDto MapToSessionDto(RefreshToken token)
        {
            var maskedId = IpAddressMasker.Mask(token.IpAddress);

            return new SessionDto
            {
                TokenHash = token.TokenHash,
                IpAddress = maskedId,
                IssuedAt = token.IssuedAt,
                ExpiresAt = token.ExpiresAt
            };
        }

        private static InvitationDto MapToInvitationDto(InvitationCode invitation)
        {
            return new InvitationDto
            {
                Code = invitation.Code,
                RedeemedBy = invitation.RedeemedByUsername,
                CreatedAt = invitation.CreatedAt,
                ExpiresAt = invitation.ExpiresAt,
                Status = GetInvitationStatus(invitation),
                IsRevoked = invitation.IsRevoked
            };
        }

        private static ActivityLogDto MapToActivityLogDto(ActivityLog activityLog)
        {
            return new ActivityLogDto
            {
                Id = activityLog.Id,
                Type = activityLog.Type,
                UserId = activityLog.UserId,
                Action = activityLog.Action,
                ActivityMessage = ActivityMessageMapper.Map(activityLog.Action, activityLog.Context),
                OccurredAt = activityLog.OccurredAt
            };
        }

        private static InvitationStatus GetInvitationStatus(InvitationCode invitation)
        {
            if (invitation.IsRevoked)
                return InvitationStatus.Revoked;

            if (invitation.IsRedeemed)
                return InvitationStatus.Used;

            if (DateTime.UtcNow >= invitation.ExpiresAt)
                return InvitationStatus.Expired;

            return InvitationStatus.Active;
        }
    }
}
