using BlockSense.Contracts.Enums.User;

namespace BlockSense.Contracts.DTOs.User
{
    /// <summary>
    /// Represents a summary of a user account.
    /// </summary>
    public sealed record UserSummaryDto
    {
        /// <summary>
        /// Unique identifier of the user.
        /// </summary>
        public uint UserId { get; init; }

        /// <summary>
        /// The username of the user.
        /// </summary>
        public string Username { get; init; } = string.Empty;

        /// <summary>
        /// The email address of the user.
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Type of the user account.
        /// </summary>
        public UserType UserType { get; init; }

        /// <summary>
        /// Indicates whether two-factor authentication (2FA) is enabled for this user.
        /// </summary>
        public bool TwoFactorEnabled { get; init; }

        /// <summary>
        /// UTC timestamp when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// UTC timestamp when the user account was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; init; }
    }
}
