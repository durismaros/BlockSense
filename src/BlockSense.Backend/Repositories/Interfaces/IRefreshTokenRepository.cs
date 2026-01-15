using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Defines data access operations for managing refresh tokens.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Retrieves a refresh token by its unique identifier.
        /// </summary>
        /// <param name="tokenId">The unique identifier of the refresh token.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The <see cref="RefreshTokenEntity"/> if found; otherwise, <c>null</c>.</returns>
        Task<RefreshTokenEntity?> GetByIdAsync(Guid tokenId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Retrieves a refresh token by its hashed token value.
        /// </summary>
        /// <param name="tokenHash">The hashed refresh token value.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The <see cref="RefreshTokenEntity"/> if found; otherwise, <c>null</c>.</returns>
        Task<RefreshTokenEntity?> GetByTokenAsync(string tokenHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all refresh tokens issued to a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A collection of <see cref="RefreshTokenEntity"/> instances associated with the specified user.</returns>
        Task<IReadOnlyList<RefreshTokenEntity>> GetByUserAsync(uint userId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Retrieves all active (non-revoked and non-expired) refresh tokens issued to a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A collection of active <see cref="RefreshTokenEntity"/> instances.</returns>
        Task<IReadOnlyList<RefreshTokenEntity>> GetActiveByUserAsync(uint userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates and persists a new refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token entity to create.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The unique identifier (<see cref="Guid"/>) of the newly created refresh token.</returns>
        Task<Guid> CreateAsync(RefreshTokenEntity refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes a specific refresh token, preventing further use.
        /// </summary>
        /// <param name="tokenId">The unique identifier of the refresh token to revoke.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RevokeAsync(Guid tokenId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Revokes all refresh tokens issued to a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RevokeAllForUserAsync(uint userId, CancellationToken cancellationToken = default);
    }
}
