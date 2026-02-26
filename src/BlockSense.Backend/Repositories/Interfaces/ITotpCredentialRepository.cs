using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface ITotpCredentialRepository
    {
        Task<TotpCredential?> GetByUserIdAsync(uint userId, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(uint userId, CancellationToken cancellationToken = default);

        Task CreateAsync(TotpCredential totpCredential, CancellationToken cancellationToken = default);

        Task UpdateAsync(TotpCredential totpCredential, CancellationToken cancellationToken = default);

        Task DeleteByUserIdAsync(uint userId, CancellationToken cancellationToken = default);
    }
}
