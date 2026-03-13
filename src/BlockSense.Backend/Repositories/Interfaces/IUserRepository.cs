using BlockSense.Backend.Entities;
using BlockSense.Contracts.Enums;

namespace BlockSense.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Defines data access operations for users.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The matching <see cref="User"/>, or <c>null</c> if not found.</returns>
        Task<User?> GetByIdAsync(uint id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a user by their username or email address.
        /// </summary>
        /// <param name="identifier">The username or email address to look up.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The matching <see cref="User"/>, or <c>null</c> if not found.</returns>
        Task<User?> GetByUsernameOrEmailAsync(string identifier, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all users assigned the specified role, ordered by creation date ascending.
        /// </summary>
        /// <param name="role">The role to filter users by.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A read-only list of users with the specified role.</returns>
        Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the username of the user who invited the specified user via an invitation code.
        /// </summary>
        /// <param name="userId">The identifier of the user whose inviter to look up.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The inviter's username, or <c>null</c> if no invitation record exists.</returns>
        Task<string?> GetInviterUsernameAsync(uint userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts a new user into the data store.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The auto-generated identifier of the newly created user.</returns>
        Task<uint> CreateAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates all mutable fields of an existing user record.
        /// </summary>
        /// <param name="user">The user containing the updated values.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft-deletes a user by setting their <c>deleted_at</c> timestamp.
        /// Only applies if the user has not already been deleted.
        /// </summary>
        /// <param name="id">The identifier of the user to soft-delete.</param>
        /// <param name="deletedAt">The UTC timestamp to record as the deletion time.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SoftDeleteAsync(uint id, DateTime deletedAt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Restores a soft-deleted user by clearing their <c>deleted_at</c> timestamp.
        /// Only applies if the user is currently soft-deleted.
        /// </summary>
        /// <param name="id">The identifier of the user to restore.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RestoreAsync(uint id, CancellationToken cancellationToken = default);
    }
}