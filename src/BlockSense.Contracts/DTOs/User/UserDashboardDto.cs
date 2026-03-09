using BlockSense.Contracts.DTOs.Invitation;
using BlockSense.Contracts.DTOs.Session;

namespace BlockSense.Contracts.DTOs.User
{
    public sealed record UserDashboardDto
    {
        public required UserSummaryDto Profile
        {
            get;
            init;
        }

        public required IEnumerable<SessionDto> ActiveTokens
        {
            get;
            init;
        }

        public required IEnumerable<ActivityLogDto> RecentActivity
        {
            get;
            init;
        }

        public required IEnumerable<InvitationDto> UserInvitations
        {
            get;
            init;
        }
    }
}
