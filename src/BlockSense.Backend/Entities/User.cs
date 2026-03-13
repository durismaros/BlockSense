using BlockSense.Contracts.Enums;

namespace BlockSense.Backend.Entities
{
    /// <summary>
    /// Represents a registered user in the system.
    /// </summary>
    public sealed class User
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public required uint Id
        {
            get;
            init;
        }

        /// <summary>
        /// The user's unique display name.
        /// </summary>
        public required string Username
        {
            get;
            set;
        }

        /// <summary>
        /// The user's email address.
        /// </summary>
        public required string Email
        {
            get;
            set;
        }

        /// <summary>
        /// The user's assigned role, determining their permissions within the system.
        /// </summary>
        public required UserRole Role
        {
            get;
            set;
        }

        /// <summary>
        /// The hashed representation of the user's password.
        /// </summary>
        public required byte[] PasswordHash
        {
            get;
            set;
        }

        /// <summary>
        /// The cryptographic salt used when hashing the user's password.
        /// </summary>
        public required byte[] PasswordSalt
        {
            get;
            set;
        }

        /// <summary>
        /// The UTC date and time at which this user account was created.
        /// </summary>
        public required DateTime CreatedAt
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC date and time at which this user account was last updated.
        /// </summary>
        public required DateTime UpdatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// The UTC date and time at which this user account was soft-deleted,
        /// or <c>null</c> if the account has not been deleted.
        /// </summary>
        public required DateTime? DeletedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether this user account is active.
        /// Returns <c>true</c> only when the account has not been deleted and the user is not banned.
        /// </summary>
        public bool IsActive
            => DeletedAt is null && Role != UserRole.Banned;

        /// <summary>
        /// Indicates whether this user account has been soft-deleted.
        /// </summary>
        public bool IsDeleted
            => DeletedAt.HasValue;

        /// <summary>
        /// Indicates whether this user has been banned from the system.
        /// </summary>
        public bool IsBanned
            => Role is UserRole.Banned;
    }
}