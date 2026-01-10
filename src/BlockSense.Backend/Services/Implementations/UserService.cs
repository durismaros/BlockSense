using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Exceptions.Registration;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.DTOs.Registration;
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
            DatabaseContext databaseContext)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
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

                if (invitation is null ||
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
                    UserId = uint.MinValue,
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
    }
}
