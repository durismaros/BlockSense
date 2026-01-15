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
        public required string Issuer
        {
            get;
            init;
        }

        /// <summary>
        /// The audience that the token is intended for (client URL).
        /// </summary>
        public required string Audience
        {
            get;
            init;
        }

        /// <summary>
        /// The duration for which the token remains valid.
        /// </summary>
        public required TimeSpan Expiration
        {
            get;
            init;
        }

        /// <summary>
        /// The secret key used to sign the JWT token.
        /// </summary>
        public required string SigningKey
        {
            get;
            init;
        }
    }
}
