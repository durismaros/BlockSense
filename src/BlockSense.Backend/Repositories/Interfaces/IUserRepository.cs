using BlockSense.Backend.Entities;
using BlockSense.Contracts.Enums.User;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserEntity?> GetByIdAsync(uint userId, CancellationToken cancellationToken = default);
        Task<UserEntity?> GetByUsernameOrEmailAsync(string identifier, CancellationToken cancellationToken = default);

        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

        Task<uint> CreateAsync(UserEntity user, CancellationToken cancellationToken = default);
        Task UpdateUserTypeAsync(uint userId, UserType newType, CancellationToken cancellationToken = default);

        Task SoftDeleteAsync(uint userId, CancellationToken cancellationToken = default);
        Task RestoreAsync(uint userId, CancellationToken cancellationToken = default);
    }
}
