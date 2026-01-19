namespace BlockSense.Contracts.DTOs.Token
{
    /// <summary>
    /// Represents a refresh token used to obtain new access tokens.
    /// </summary>
    public sealed record RefreshTokenDto
    {
        /// <summary>
        /// The raw token data, optionally transferred to the client.
        /// </summary>
        public required string Token
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
    }
}
