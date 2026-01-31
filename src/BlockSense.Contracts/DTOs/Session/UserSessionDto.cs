namespace BlockSense.Contracts.DTOs.Session
{
    /// <summary>
    /// Represents a device session associated with a user account.
    /// </summary>
    public sealed record UserSessionDto
    {
        /// <summary>
        /// Unique identifier of the device session (Hashed value of the refresh token).
        /// </summary>
        public required string TokenHash
        {
            get;
            init;
        }

        /// <summary>
        /// IP address from which the token was initiated.
        /// </summary>
        public required string IpAddress
        {
            get;
            init;
        }

        /// <summary>
        /// UTC timestamp when the token was issued.
        /// </summary>
        public required DateTime IssuedAt
        {
            get;
            init;
        }

        /// <summary>
        /// UTC timestamp when the token expires.
        /// </summary>
        public required DateTime ExpiresAt
        {
            get;
            init;
        }
    }
}
