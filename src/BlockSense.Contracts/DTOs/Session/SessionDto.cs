namespace BlockSense.Contracts.DTOs.Session
{
    /// <summary>
    /// Represents a device session associated with a user account.
    /// </summary>
    public sealed record SessionDto
    {
        /// <summary>
        /// The hashed refresh token value that uniquely identifies this session.
        /// </summary>
        public required string TokenHash
        {
            get;
            init;
        }

        /// <summary>
        /// The IP address from which this session was initiated.
        /// </summary>
        public required string IpAddress
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC timestamp when this session was issued.
        /// </summary>
        public required DateTime IssuedAt
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC timestamp when this session expires.
        /// </summary>
        public required DateTime ExpiresAt
        {
            get;
            init;
        }
    }
}