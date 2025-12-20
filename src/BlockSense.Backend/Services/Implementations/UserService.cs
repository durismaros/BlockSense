using BlockSense.Backend.Data;
using BlockSense.Backend.Entities;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.Cryptography.Hashing;
using BlockSense.Contracts.Cryptography.Utils;
using BlockSense.Contracts.DTOs.Auth.Register;
using BlockSense.Contracts.Enums.Auth;
using System.Text;

namespace BlockSense.Backend.Services.Implementations
{
    public sealed class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IInvitationRepository _invitationRepository;
        private readonly DatabaseContext _databaseContext;

        public UserService(
            IUserRepository userRepository,
            IInvitationRepository invitationRepository,
            DatabaseContext databaseContext)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        public async Task<RegistrationResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
        {
            await _databaseContext.BeginTransactionAsync(cancellationToken: cancellationToken);

            uint invitationCodeId = await _invitationRepository.GetByCodeForUpdateAsync(request.InvitationCode, cancellationToken);

            if (invitationCodeId < 1)
            {
                await _databaseContext.RollbackAsync(cancellationToken);
                return new RegistrationResponse { Status = RegistrationStatus.InvalidInvitationCode, Message = "Invalid or inactive invitation code." };
            }


            if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken))
            {
                await _databaseContext.RollbackAsync(cancellationToken);
                return new RegistrationResponse { Status = RegistrationStatus.UsernameTaken, Message = "Username already in use." };
            }

            if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            {
                await _databaseContext.RollbackAsync(cancellationToken);
                return new RegistrationResponse { Status = RegistrationStatus.EmailTaken, Message = "Email already registered." };
            }


            try
            {
                var argon2IdHashed = new Argon2idHasher();

                var passwordSalt = CryptographyUtilities.GenerateSecureRandomBytes(argon2IdHashed.SaltLength);
                var passwordHash = argon2IdHashed.Derive(
                    Encoding.UTF8.GetBytes(request.Password),
                    out _,
                    providedSalt: passwordSalt);

                var now = DateTime.UtcNow;

                var userEntity = new UserEntity
                {
                    Username = request.Username,
                    Email = request.Email,
                    UserType = Contracts.Enums.User.UserType.Standard,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    InvitationCodeId = invitationCodeId,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                uint userId = await _userRepository.CreateAsync(userEntity, cancellationToken);

                await _invitationRepository.MarkAsUsedAsync(invitationCodeId, cancellationToken);

                await _databaseContext.CommitAsync(cancellationToken);
                return new RegistrationResponse
                {
                    Status = RegistrationStatus.Success,
                    UserId = userId,
                    Message = "User registered successfully."
                };
            }
            catch
            {
                await _databaseContext.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
