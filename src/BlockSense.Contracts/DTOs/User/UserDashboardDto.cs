using BlockSense.Contracts.DTOs.Invitation;
using BlockSense.Contracts.DTOs.Session;

namespace BlockSense.Contracts.DTOs.User
{
    /// <summary>
    /// Represents a consolidated view of a user's profile, statistics, active devices, and invitations.
    /// </summary>
    public sealed record UserDashboardDto
    {
        /// <summary>
        /// Basic profile information about the user.
        /// </summary>
        public required UserSummaryDto Profile
        {
            get;
            init;
        }

        /// <summary>
        /// List of currently active devices/sessions for the user.
        /// </summary>
        public required IList<SessionDto> ActiveTokens
        {
            get;
            init;
        }

        /// <summary>
        /// List of invitation codes created by the user.
        /// </summary>
        public required IReadOnlyList<InvitationDto> UserInvitations
        {
            get;
            init;
        }
    }
}
