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
        public required string Issuer
        {
            get;
            init;
        }

        /// <summary>
        /// Master secret key used as a base for generating user-specific 2FA secrets.
        /// </summary>
        public required string MasterKey
        {
            get;
            init;
        }

        /// <summary>
        /// Number of digits used for generated 2FA verification codes.
        /// </summary>
        public required int CodeLength
        {
            get;
            init;
        }

        /// <summary>
        /// The duration for which a generated 2FA code remains valid.
        /// </summary>
        public required TimeSpan CodeLifetime
        {
            get;
            init;
        }

        /// <summary>
        /// Number of backup codes generated for account recovery.
        /// </summary>
        public required int BackupCodeCount
        {
            get;
            init;
        }

        /// <summary>
        /// Length of each backup code.
        /// </summary>
        public required int BackupCodeLength
        {
            get;
            init;
        }
    }
}
