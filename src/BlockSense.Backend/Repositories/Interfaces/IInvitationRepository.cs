using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Defines data access operations for invitation codes.
    /// </summary>
    public interface IInvitationRepository
    {
        /// <summary>
        /// Retrieves an invitation code by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the invitation code.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The matching <see cref="InvitationCode"/>, or <c>null</c> if not found.</returns>
        Task<InvitationCode?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an invitation code by its code string.
        /// </summary>
        /// <param name="code">The invitation code string to look up.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The matching <see cref="InvitationCode"/>, or <c>null</c> if not found.</returns>
        Task<InvitationCode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an invitation code by its code string with a pessimistic row lock,
        /// intended for use within a transaction to prevent concurrent redemptions.
        /// </summary>
        /// <param name="code">The invitation code string to look up.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The matching <see cref="InvitationCode"/>, or <c>null</c> if not found.</returns>
        Task<InvitationCode?> GetByCodeForUpdateAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all invitation codes issued to the specified user,
        /// ordered with unredeemed codes first, then by creation date ascending.
        /// </summary>
        /// <param name="issuedToId">The identifier of the user to whom codes were issued.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A read-only list of invitation codes issued to the user.</returns>
        Task<IReadOnlyList<InvitationCode>> GetByIssuedToIdAsync(uint issuedToId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts a new invitation code into the data store.
        /// </summary>
        /// <param name="invitationCode">The invitation code to create.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The auto-generated identifier of the newly created invitation code.</returns>
        Task<uint> CreateAsync(InvitationCode invitationCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks an invitation code as redeemed by the specified user.
        /// Only updates the record if the code is currently unredeemed, not revoked, and not expired.
        /// </summary>
        /// <param name="id">The identifier of the invitation code to redeem.</param>
        /// <param name="redeemedById">The identifier of the user redeeming the code.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RedeemAsync(uint id, uint redeemedById, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks an invitation code as revoked, preventing further use.
        /// </summary>
        /// <param name="id">The identifier of the invitation code to revoke.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RevokeAsync(uint id, CancellationToken cancellationToken = default);
    }
}