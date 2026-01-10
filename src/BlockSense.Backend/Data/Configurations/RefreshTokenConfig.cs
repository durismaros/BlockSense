namespace BlockSense.Backend.Data.Configurations
{
    /// <summary>
    /// Configuration settings for refresh token behavior.
    /// </summary>
    public sealed record RefreshTokenConfig
    {
        /// <summary>
        /// The duration for which a refresh token remains valid.
        /// </summary>
        /// <remarks>Once expired, the user must re-authenticate to obtain a new refresh token.</remarks>
        public required TimeSpan Expiration
        {
            get;
            init;
        }
    }
}
