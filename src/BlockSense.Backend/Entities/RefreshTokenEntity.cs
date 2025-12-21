namespace BlockSense.Backend.Entities
{
    /// <summary>
    /// Represents a refresh token associated with a user and a specific device/session.
    /// </summary>
    public sealed class RefreshTokenEntity
    {
        /// <summary>
        /// Primary key of the refresh token in GUID v4 format.
        /// </summary>
        public Guid TokenId { get; set; }

        /// <summary>
        /// Foreign key referencing the user who owns the refresh token.
        /// </summary>
        public uint UserId { get; set; }

        /// <summary>
        /// Hashed value of the refresh token for secure storage.
        /// </summary>
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>
        /// IP address from which the refresh token was issued. Supports both IPv4 and IPv6.
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable device label
        /// </summary>
        public string DeviceIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// Unique hardware fingerprint of the device, encoded in Base64.
        /// </summary>
        public string HardwareFingerprint { get; set; } = string.Empty;

        /// <summary>
        /// Short network fingerprint, Base64-encoded, used to help detect suspicious sessions.
        /// </summary>
        public string NetworkFingerprint { get; set; } = string.Empty;

        /// <summary>
        /// Operating system of the device.
        /// </summary>
        public string DeviceOs { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the refresh token was issued.
        /// </summary>
        public DateTime IssuedAt { get; set; }

        /// <summary>
        /// UTC timestamp when the refresh token will expire.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Indicates whether the token has been revoked manually or by policy.
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// Returns <c>true</c> if the token is currently active (not revoked and not expired); otherwise, <c>false</c>.
        /// </summary>
        public bool IsActive => !IsRevoked && ExpiresAt > DateTime.UtcNow;

        /// <summary>
        /// Gets the remaining time until the token expires. Can be negative if the token has already expired.
        /// </summary>
        public TimeSpan TimeUntilExpiration => ExpiresAt - DateTime.UtcNow;
    }
}
