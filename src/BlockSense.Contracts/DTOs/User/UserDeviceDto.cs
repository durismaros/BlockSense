using BlockSense.Contracts.Enums.User;

namespace BlockSense.Contracts.DTOs.User
{
    /// <summary>
    /// Represents a device session associated with a user account.
    /// </summary>
    public sealed record UserDeviceDto
    {
        /// <summary>
        /// Unique identifier of the device session (refresh token ID).
        /// </summary>
        public Guid TokenId { get; init; }

        /// <summary>
        /// Current status of the device session.
        /// </summary>
        public UserDeviceStatus Status { get; init; }

        /// <summary>
        /// Name or label of the device associated with the token.
        /// </summary>
        public string DeviceName { get; init; } = string.Empty;

        /// <summary>
        /// IP address from which the token was initiated.
        /// </summary>
        public string IpAddress { get; init; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the token was issued.
        /// </summary>
        public DateTime IssuedAt { get; init; }

        /// <summary>
        /// UTC timestamp when the token expires.
        /// </summary>
        public DateTime ExpiresAt { get; init; }
    }
}
