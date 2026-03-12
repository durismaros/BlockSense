namespace BlockSense.Contracts.DTOs.Token
{
    /// <summary>
    /// Represents a refresh token used to obtain new access tokens.
    /// </summary>
    public sealed record RefreshTokenDto
    {
        /// <summary>
        /// The raw refresh token string.
        /// </summary>
        public required string Token
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC timestamp at which this token expires.
        /// </summary>
        public required DateTime ExpiresAt
        {
            get;
            init;
        }
    }
}