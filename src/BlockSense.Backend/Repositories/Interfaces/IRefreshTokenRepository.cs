using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Defines data access operations for refresh tokens.
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Retrieves a refresh token by its hashed value.
        /// </summary>
        /// <param name="tokenHash">The hashed token value to look up.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The matching <see cref="RefreshToken"/>, or <c>null</c> if not found.</returns>
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a refresh token by the hardware fingerprint of the issuing device.
        /// </summary>
        /// <param name="hardwareFingerprint">The hardware fingerprint to look up.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The matching <see cref="RefreshToken"/>, or <c>null</c> if not found.</returns>
        Task<RefreshToken?> GetByHardwareFingerprintAsync(string hardwareFingerprint, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all active (non-revoked and non-expired) refresh tokens for the specified user,
        /// ordered by issuance date descending.
        /// </summary>
        /// <param name="userId">The identifier of the user whose active tokens to retrieve.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A read-only list of active refresh tokens for the user.</returns>
        Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(uint userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts or replaces a refresh token in the data store.
        /// An existing token for the same hardware fingerprint will be overwritten.
        /// </summary>
        /// <param name="refreshToken">The refresh token to create.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes the refresh token with the specified hash.
        /// </summary>
        /// <param name="tokenHash">The hashed value of the token to revoke.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RevokeAsync(string tokenHash, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes all active refresh tokens belonging to the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose tokens to revoke.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RevokeAllByUserIdAsync(uint userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Permanently deletes all refresh tokens that have passed their expiration date.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
    }
}