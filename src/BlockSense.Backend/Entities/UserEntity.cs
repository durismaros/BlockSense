using BlockSense.Contracts.Enums.User;

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
        public uint UserId { get; set; }

        /// <summary>
        /// Unique username chosen by the user.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Unique email address of the user.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Type of user account (e.g., Standard, Administrator, Banned).
        /// </summary>
        public UserType UserType { get; set; } = UserType.None;

        /// <summary>
        /// Password hash stored as a 32-byte Argon2 hash.
        /// </summary>
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Random 16-byte salt used for hashing the password.
        /// </summary>
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// The ID of the invitation code used during registration.
        /// </summary>
        public uint InvitationCodeId { get; set; }

        /// <summary>
        /// Timestamp when the user account was created (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the user account was last updated (UTC).
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete timestamp. Null indicates the account is active.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Indicates whether the user account has been soft-deleted.
        /// </summary>
        public bool IsDeleted => DeletedAt.HasValue;
    }
}
