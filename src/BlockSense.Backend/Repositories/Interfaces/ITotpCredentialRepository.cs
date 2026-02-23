using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface ITotpCredentialRepository
    {
        Task<TotpCredential?> GetByUserIdAsync(uint userId, CancellationToken cancellationToken = default);

        Task<bool> IsEnabledAsync(uint userId, CancellationToken cancellationToken = default);

        Task CreateAsync(TotpCredential credential, CancellationToken cancellationToken = default);

        Task UpdateBackupCodesAsync(uint userId, IEnumerable<string> backupCodes, CancellationToken cancellationToken = default);

        Task DeleteAsync(uint userId, CancellationToken cancellationToken = default);
    }
}
