using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshTokenEntity?> GetByIdAsync(Guid tokenId, CancellationToken cancellationToken = default);
        Task<RefreshTokenEntity?> GetByTokenAsync(string tokenHash, CancellationToken cancellationToken = default);

        Task<IEnumerable<RefreshTokenEntity>> GetByUserAsync(uint userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RefreshTokenEntity>> GetActiveByUserAsync(uint userId, CancellationToken cancellationToken = default);

        Task<Guid> CreateAsync(RefreshTokenEntity refreshToken, CancellationToken cancellationToken = default);

        Task RevokeAsync(Guid tokenId, CancellationToken cancellationToken = default);
        Task RevokeAllForUserAsync(uint userId, CancellationToken cancellationToken = default);
    }
}
