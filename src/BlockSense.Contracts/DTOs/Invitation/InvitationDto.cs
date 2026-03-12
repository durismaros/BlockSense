using BlockSense.Contracts.Enums;

namespace BlockSense.Contracts.DTOs.Invitation
{
    /// <summary>
    /// Represents an invitation code and its associated metadata.
    /// </summary>
    public sealed record InvitationDto
    {
        /// <summary>
        /// The unique invitation code.
        /// </summary>
        public required string Code
        {
            get;
            init;
        }

        /// <summary>
        /// The username of the user who redeemed this invitation, or null if not yet redeemed.
        /// </summary>
        public required string? RedeemedBy
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC timestamp when the invitation was created.
        /// </summary>
        public required DateTime CreatedAt
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC timestamp when the invitation expires.
        /// </summary>
        public required DateTime ExpiresAt
        {
            get;
            init;
        }

        /// <summary>
        /// The current status of the invitation.
        /// </summary>
        public required InvitationStatus Status
        {
            get;
            init;
        }

        /// <summary>
        /// Indicates whether the invitation has been manually revoked.
        /// </summary>
        public required bool IsRevoked
        {
            get;
            init;
        }
    }
}