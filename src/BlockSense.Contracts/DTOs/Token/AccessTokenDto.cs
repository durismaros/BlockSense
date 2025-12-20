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
        public string Token { get; init; } = string.Empty;

        /// <summary>
        /// The UTC time at which this token expires.
        /// </summary>
        public DateTime ExpiresAt { get; init; }
    }
}
