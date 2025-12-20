using BlockSense.Contracts.Enums.User;

namespace BlockSense.Contracts.DTOs.User
{
    /// <summary>
    /// Represents an invitation code and its current state.
    /// Used to display invitation information to clients and for administrative purposes.
    /// </summary>
    public sealed record InvitationDto
    {
        /// <summary>
        /// The unique invitation code string.
        /// </summary>
        public string InvitationCode { get; init; } = string.Empty;

        /// <summary>
        /// Current status of the invitation.
        /// </summary>
        public InvitationStatus Status { get; init; } = InvitationStatus.Unknown;

        /// <summary>
        /// The UTC date and time when the invitation was created.
        /// </summary>
        public DateTime CreationDate { get; init; }

        /// <summary>
        /// The UTC date and time when the invitation will expire.
        /// </summary>
        public DateTime ExpirationDate { get; init; }

        /// <summary>
        /// The username or identifier of the user who generated this invitation.
        /// </summary>
        public string GeneratedBy { get; init; } = string.Empty;

        /// <summary>
        /// The username or identifier of the user who used this invitation.
        /// </summary>
        public string? InvitedUser { get; init; }
    }
}
