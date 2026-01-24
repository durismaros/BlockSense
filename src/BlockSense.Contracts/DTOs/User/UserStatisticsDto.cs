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
        public required uint UserId
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
        /// The Username of the user who invited this user.
        /// </summary>
        public required string InvitedBy
        {
            get;
            init;
        }

        /// <summary>
        /// Total number of users invited by this user.
        /// </summary>
        public required int TotalInvitedUsers
        {
            get;
            init;
        }

        /// <summary>
        /// UTC timestamp of the user's last login, if available.
        /// </summary>
        public DateTime? LastLoginAt
        {
            get;
            init;
        }

        /// <summary>
        /// Number of currently active devices/sessions associated with this user.
        /// </summary>
        public required int ActiveDeviceCount
        {
            get;
            init;
        }
    }
}
