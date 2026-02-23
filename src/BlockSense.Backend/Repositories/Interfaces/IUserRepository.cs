using BlockSense.Backend.Entities;
using BlockSense.Contracts.Enums;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(uint userId, CancellationToken cancellationToken = default);

        Task<User?> GetByUsernameOrEmailAsync(string identifier, CancellationToken cancellationToken = default);

        Task<string?> GetInviterUsernameByUserAsync(uint userId, CancellationToken cancellationToken = default);

        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

        Task<uint> CreateAsync(User user, CancellationToken cancellationToken = default);

        Task UpdateRoleAsync(uint userId, UserRole role, CancellationToken cancellationToken = default);

        Task SoftDeleteAsync(uint userId, CancellationToken cancellationToken = default);

        Task RestoreAsync(uint userId, CancellationToken cancellationToken = default);
    }
}
