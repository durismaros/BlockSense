using BlockSense.Contracts.Enums.User;

namespace BlockSense.Contracts.DTOs.User
{
    /// <summary>
    /// Represents statistical information about a user account.
    /// </summary>
    public sealed record UserStatisticsDto
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public uint UserId { get; init; }

        /// <summary>
        /// Type of the user account.
        /// </summary>
        public UserType UserType { get; init; }

        /// <summary>
        /// UTC timestamp when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// UTC timestamp of the user's last login, if available.
        /// </summary>
        public DateTime? LastLoginAt { get; init; }

        /// <summary>
        /// Indicates whether two-factor authentication (2FA) is enabled for the user.
        /// </summary>
        public bool TwoFactorEnabled { get; init; }

        /// <summary>
        /// Total number of users invited by this user.
        /// </summary>
        public int TotalInvitedUsers { get; init; }

        /// <summary>
        /// Number of currently active devices/sessions associated with this user.
        /// </summary>
        public int ActiveDeviceCount { get; init; }
    }
}
