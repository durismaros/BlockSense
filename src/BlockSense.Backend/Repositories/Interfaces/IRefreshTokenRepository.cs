using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

        Task<RefreshToken?> GetByHardwareFingerprintAsync(string hardwareFingerprint, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(uint userId, CancellationToken cancellationToken = default);

        Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

        Task RevokeAsync(string tokenHash, CancellationToken cancellationToken = default);

        Task RevokeAllByUserIdAsync(uint userId, CancellationToken cancellationToken = default);

        Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
    }
}
