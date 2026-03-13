using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Defines data access operations for TOTP credentials.
    /// </summary>
    public interface ITotpCredentialRepository
    {
        /// <summary>
        /// Retrieves the TOTP credential associated with the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose credential to retrieve.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The user's <see cref="TotpCredential"/>, or <c>null</c> if none exists.</returns>
        Task<TotpCredential?> GetByUserIdAsync(uint userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines whether a TOTP credential exists for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user to check.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns><c>true</c> if a TOTP credential exists for the user; otherwise <c>false</c>.</returns>
        Task<bool> ExistsAsync(uint userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts a new TOTP credential into the data store.
        /// </summary>
        /// <param name="totpCredential">The TOTP credential to create.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CreateAsync(TotpCredential totpCredential, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the encrypted secret and backup codes of an existing TOTP credential.
        /// </summary>
        /// <param name="totpCredential">The TOTP credential containing the updated values.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateAsync(TotpCredential totpCredential, CancellationToken cancellationToken = default);

        /// <summary>
        /// Permanently deletes the TOTP credential associated with the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose credential to delete.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task DeleteByUserIdAsync(uint userId, CancellationToken cancellationToken = default);
    }
}