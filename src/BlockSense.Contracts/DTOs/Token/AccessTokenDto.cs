namespace BlockSense.Contracts.DTOs.Token
{
    /// <summary>
    /// Represents an access token used for API authentication along with its expiration metadata.
    /// </summary>
    public sealed record AccessTokenDto
    {
        /// <summary>
        /// The JWT or opaque token string used for API authentication.
        /// </summary>
        public required string Token
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC time at which this token expires.
        /// </summary>
        public required DateTime ExpiresAt
        {
            get;
            init;
        }
    }
}
