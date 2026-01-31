using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Defines data access operations for managing user two-factor authentication.
    /// </summary>
    public interface ITwoFactorAuthRepository
    {
        /// <summary>
        /// Retrieves the 2FA entity for a specific user by their <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The <see cref="TwoFactorAuthEntity"/> if found; otherwise, <c>null</c>.</returns>
        Task<TwoFactorAuthEntity?> GetByUserIdAsync(uint userId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Checks whether a user has Two-Factor authentication enabled.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns><c>true</c> if 2FA is enabled; otherwise, <c>false</c>.</returns>
        Task<bool> IsEnabledAsync(uint userId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Inserts a new 2FA entity.
        /// </summary>
        /// <param name="entity">The 2FA entity to create or update.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateAsync(TwoFactorAuthEntity entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="backupCodes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task UpdateBackupCodesAsync(uint userId, IReadOnlyList<string> backupCodes, CancellationToken cancellationToken = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="backupCodes"></param>
        /// <param name="updatedAt"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task InsertBackupCodesAsync(uint userId, IReadOnlyList<string> backupCodes, DateTime updatedAt, CancellationToken cancellationToken = default);
        /// <summary>
        /// Deletes the 2FA data for a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose 2FA data should be deleted.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DisableAsync(uint userId, CancellationToken cancellationToken = default);
    }
}
