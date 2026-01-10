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
        /// Aggregated statistics about the user account.
        /// </summary>
        public required UserStatisticsDto Statistics
        {
            get;
            init;
        }

        /// <summary>
        /// List of currently active devices/sessions for the user.
        /// </summary>
        public required IReadOnlyList<UserDeviceDto> ActiveDevices
        {
            get;
            init;
        }

        /// <summary>
        /// List of invitation codes created by the user.
        /// </summary>
        public required IReadOnlyList<InvitationDto> Invitations
        {
            get;
            init;
        }
    }
}
