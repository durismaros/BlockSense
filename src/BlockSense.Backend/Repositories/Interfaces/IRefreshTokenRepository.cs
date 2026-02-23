using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RefreshToken>> GetByUserAsync(uint userId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(uint userId, CancellationToken cancellationToken = default);

        Task UpsertAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

        Task RevokeAsync(string tokenHash, CancellationToken cancellationToken = default);

        Task RevokeAllForUserAsync(uint userId, CancellationToken cancellationToken = default);
    }
}
