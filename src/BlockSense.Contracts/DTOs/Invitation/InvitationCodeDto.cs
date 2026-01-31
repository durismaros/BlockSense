using BlockSense.Contracts.Enums.User;

namespace BlockSense.Contracts.DTOs.Invitation
{
    /// <summary>
    /// Represents an invitation code and its current state.
    /// Used to display invitation information to clients and for administrative purposes.
    /// </summary>
    public sealed record InvitationCodeDto
    {
        /// <summary>
        /// The unique invitation code string.
        /// </summary>
        public required string InvitationCode
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC date and time when the invitation was created.
        /// </summary>
        public required DateTime CreatedAt
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC date and time when the invitation will expire.
        /// </summary>
        public required DateTime ExpiresAt
        {
            get;
            init;
        }

        /// <summary>
        /// The username or identifier of the user who used this invitation.
        /// </summary>
        public string? InvitedUser
        {
            get;
            init;
        }

        /// <summary>
        /// Current status of the invitation.
        /// </summary>
        public required InvitationStatus Status
        {
            get;
            init;
        }
    }
}
