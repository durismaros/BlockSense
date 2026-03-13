using BlockSense.Contracts.Enums;

namespace BlockSense.Contracts.DTOs.User
{
    /// <summary>
    /// Represents a summary of a user's account information.
    /// </summary>
    public sealed record UserSummaryDto
    {
        /// <summary>
        /// The unique identifier of the user.
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
        /// The role assigned to the user.
        /// </summary>
        public required UserRole Role
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC timestamp when the user account was created.
        /// </summary>
        public required DateTime CreatedAt
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC timestamp when the user account was last updated.
        /// </summary>
        public required DateTime UpdatedAt
        {
            get;
            init;
        }

        /// <summary>
        /// The username of the user who invited this account, if applicable.
        /// </summary>
        public required string InvitedBy
        {
            get;
            init;
        }

        /// <summary>
        /// Indicates whether two-factor authentication is enabled for this account.
        /// </summary>
        public required bool TwoFactorEnabled
        {
            get;
            init;
        }
    }
}