namespace BlockSense.Backend.Data.Configurations
{
    /// <summary>
    /// Configuration settings for JWT authentication.
    /// </summary>
    public sealed record JwtTokenConfig
    {
        /// <summary>
        /// The issuer of the JWT token (application).
        /// </summary>
        public string Issuer { get; init; } = string.Empty;

        /// <summary>
        /// The audience that the token is intended for (client URL).
        /// </summary>
        public string Audience { get; init; } = string.Empty;

        /// <summary>
        /// The secret key used to sign the JWT token.
        /// </summary>
        public string SigningKey { get; init; } = string.Empty;

        /// <summary>
        /// The duration for which the token remains valid.
        /// </summary>
        public TimeSpan Expiration { get; init; }
    }
}
