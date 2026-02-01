using BlockSense.Backend.Entities;
using BlockSense.Contracts.Enums;

namespace BlockSense.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Defines data access operations for managing user entities.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves a user by its unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The corresponding <see cref="UserEntity"/> if found; otherwise, <c>null</c>.</returns>
        Task<UserEntity?> GetByIdAsync(uint userId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Retrieves a user by username or email address.
        /// </summary>
        /// <param name="identifier">The username or email associated with the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The corresponding <see cref="UserEntity"/> if found; otherwise, <c>null</c>.</returns>
        Task<UserEntity?> GetByUsernameOrEmailAsync(string identifier, CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines whether a username already exists.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns><c>true</c> if the username is already in use; otherwise, <c>false</c>.</returns>
        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
        /// <summary>
        /// Determines whether an email address is already associated with an account.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns><c>true</c> if the email is already in use; otherwise, <c>false</c>.</returns>
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        /// <param name="user">The user entity to persist.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The unique identifier of the newly created user.</returns>
        Task<uint> CreateAsync(UserEntity user, CancellationToken cancellationToken = default);
        /// <summary>
        /// Updates the user type for an existing account.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="newType">The new user type to assign.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateUserTypeAsync(uint userId, UserType newType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks a user account as deleted without permanently removing it.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SoftDeleteAsync(uint userId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Restores a previously soft-deleted user account.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task RestoreAsync(uint userId, CancellationToken cancellationToken = default);
    }
}
