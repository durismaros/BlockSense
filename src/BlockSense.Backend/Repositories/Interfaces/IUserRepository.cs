using BlockSense.Backend.Entities;
using BlockSense.Contracts.Enums;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

        Task<User?> GetByUsernameOrEmailAsync(string identifier, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);

        Task<string?> GetInviterUsernameAsync(uint userId, CancellationToken cancellationToken = default);

        Task<uint> CreateAsync(User user, CancellationToken cancellationToken = default);

        Task UpdateAsync(User user, CancellationToken cancellationToken = default);

        Task SoftDeleteAsync(uint id, DateTime deletedAt, CancellationToken cancellationToken = default);

        Task RestoreAsync(uint id, CancellationToken cancellationToken = default);
    }
}
