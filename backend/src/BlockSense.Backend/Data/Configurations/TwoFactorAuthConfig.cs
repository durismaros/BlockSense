namespace BlockSense.Backend.Data.Configurations
{
    /// <summary>
    /// Configuration settings for two-factor authentication (2FA).
    /// </summary>
    public sealed record TwoFactorAuthConfig
    {
        /// <summary>
        /// The issuer name displayed in the authenticator application.
        /// </summary>
        public string Issuer { get; init; } = string.Empty;

        /// <summary>
        /// Master secret key used as a base for generating user-specific 2FA secrets.
        /// </summary>
        public string MasterKey { get; init; } = string.Empty;

        /// <summary>
        /// Number of digits used for generated 2FA verification codes.
        /// </summary>
        public int CodeLength { get; init; }

        /// <summary>
        /// The duration for which a generated 2FA code remains valid.
        /// </summary>
        public TimeSpan CodeLifetime {  get; init; }

        /// <summary>
        /// Number of backup codes generated for account recovery.
        /// </summary>
        public int BackupCodeCount { get; init; }

        /// <summary>
        /// Length of each backup code.
        /// </summary>
        public int BackupCodeLength { get; init; }
    }
}
