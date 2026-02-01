using BlockSense.Contracts.Enums;

namespace BlockSense.Backend.Entities
{
    /// <summary>
    /// Represents a user account stored in the database.
    /// </summary>
    public sealed class UserEntity
    {
        /// <summary>
        /// Primary key of the user.
        /// </summary>
        public required uint UserId
        {
            get;
            set;
        }

        /// <summary>
        /// Unique username chosen by the user.
        /// </summary>
        public required string Username
        {
            get;
            set;
        }

        /// <summary>
        /// Unique email address of the user.
        /// </summary>
        public required string Email
        {
            get;
            set;
        }

        /// <summary>
        /// Type of user account (e.g., Standard, Administrator, Banned).
        /// </summary>
        public required UserType UserType
        {
            get;
            set;
        }

        /// <summary>
        /// Password hash stored as a 32-byte Argon2 hash.
        /// </summary>
        public required byte[] PasswordHash
        {
            get;
            set;
        }

        /// <summary>
        /// Random 16-byte salt used for hashing the password.
        /// </summary>
        public required byte[] PasswordSalt
        {
            get;
            set;
        }

        /// <summary>
        /// Timestamp when the user account was created (UTC).
        /// </summary>
        public required DateTime CreatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Timestamp when the user account was last updated (UTC).
        /// </summary>
        public required DateTime UpdatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Soft delete timestamp. Null indicates the account is active.
        /// </summary>
        public DateTime? DeletedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the user account has been soft-deleted.
        /// </summary>
        public bool IsDeleted
            => DeletedAt.HasValue;
    }
}
