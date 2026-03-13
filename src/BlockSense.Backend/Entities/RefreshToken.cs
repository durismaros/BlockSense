namespace BlockSense.Backend.Entities
{
    /// <summary>
    /// Represents a refresh token issued to an authenticated user for session continuation.
    /// </summary>
    public sealed class RefreshToken
    {
        /// <summary>
        /// The hashed value of the refresh token used for secure storage and lookup.
        /// </summary>
        public required string TokenHash
        {
            get;
            init;
        }

        /// <summary>
        /// The identifier of the user to whom this refresh token was issued.
        /// </summary>
        public required uint UserId
        {
            get;
            init;
        }

        /// <summary>
        /// The IP address from which the token was originally issued.
        /// </summary>
        public required string IpAddress
        {
            get;
            init;
        }

        /// <summary>
        /// A unique identifier for the device from which the token was issued.
        /// </summary>
        public required string DeviceIdentifier
        {
            get;
            init;
        }

        /// <summary>
        /// The operating system of the device from which the token was issued.
        /// </summary>
        public required string DeviceOs
        {
            get;
            init;
        }

        /// <summary>
        /// A fingerprint derived from hardware characteristics of the issuing device.
        /// </summary>
        public required string HardwareFingerprint
        {
            get;
            init;
        }

        /// <summary>
        /// A fingerprint derived from network characteristics at the time of issuance.
        /// </summary>
        public required string NetworkFingerprint
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC date and time at which this token was issued.
        /// </summary>
        public required DateTime IssuedAt
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC date and time at which this token expires.
        /// </summary>
        public required DateTime ExpiresAt
        {
            get;
            init;
        }

        /// <summary>
        /// Indicates whether this token has been manually revoked.
        /// </summary>
        public required bool IsRevoked
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether this token has passed its expiration date.
        /// </summary>
        public bool IsExpired
            => DateTime.UtcNow >= ExpiresAt;

        /// <summary>
        /// Indicates whether this token can still be used to obtain a new access token.
        /// Returns <c>true</c> only when the token is not revoked and not expired.
        /// </summary>
        public bool IsValid
            => !IsRevoked && !IsExpired;

        /// <summary>
        /// The remaining time before this token expires.
        /// Returns a negative value if the token has already expired.
        /// </summary>
        public TimeSpan TimeUntilExpiration
            => ExpiresAt - DateTime.UtcNow;
    }
}