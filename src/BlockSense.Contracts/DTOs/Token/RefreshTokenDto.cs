namespace BlockSense.Contracts.DTOs.Token
{
    /// <summary>
    /// Represents a refresh token used to obtain new access tokens.
    /// </summary>
    public sealed record RefreshTokenDto
    {
        /// <summary>
        /// Unique identifier of the refresh token.
        /// </summary>
        public Guid TokenId { get; init; }

        /// <summary>
        /// The raw token data, optionally transferred to the client.
        /// </summary>
        public string? Token { get; init; }

        /// <summary>
        /// Unique identifier of the user associated with this refresh token.
        /// </summary>
        public uint UserId { get; init; }

        /// <summary>
        /// The UTC date and time at which this token was issued.
        /// </summary>
        public DateTimeOffset IssuedAt { get; init; }

        /// <summary>
        /// The UTC date and time at which this token expires.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; init; }
    }
}
