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
        public required uint UserId
        {
            get;
            init;
        }

        /// <summary>
        /// The username of the user.
        /// </summary>
        public required string Username
        {
            get;
            init;
        }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        public required string Email
        {
            get;
            init;
        }

        /// <summary>
        /// Type of the user account.
        /// </summary>
        public required UserType UserType
        {
            get;
            init;
        }

        /// <summary>
        /// Indicates whether two-factor authentication (2FA) is enabled for this user.
        /// </summary>
        public required bool TwoFactorEnabled
        {
            get;
            init;
        }

        /// <summary>
        /// UTC timestamp when the user account was created.
        /// </summary>
        public required DateTime CreatedAt
        {
            get;
            init;
        }

        /// <summary>
        /// UTC timestamp when the user account was last updated.
        /// </summary>
        public required DateTime UpdatedAt
        {
            get;
            init;
        }
    }
}
