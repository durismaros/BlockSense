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
        public UserSummaryDto Profile { get; init; } = new();

        /// <summary>
        /// Aggregated statistics about the user account.
        /// </summary>
        public UserStatisticsDto Statistics { get; init; } = new();

        /// <summary>
        /// List of currently active devices/sessions for the user.
        /// </summary>
        public IReadOnlyList<UserDeviceDto> ActiveDevices { get; init; } = Array.Empty<UserDeviceDto>();

        /// <summary>
        /// List of invitation codes created by the user.
        /// </summary>
        public IReadOnlyList<InvitationDto> Invitations { get; init; } = Array.Empty<InvitationDto>();
    }
}
