using System.ComponentModel.DataAnnotations;

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
        [Required]
        public required string Issuer
        {
            get;
            init;
        }

        /// <summary>
        /// Number of digits used for generated 2FA verification codes.
        /// </summary>
        [Required]
        [Range(6, 8, ErrorMessage = "CodeLength must be between 6 and 8 digits.")]
        public required int CodeLength
        {
            get;
            init;
        }

        /// <summary>
        /// The duration for which a generated 2FA code remains valid.
        /// </summary>
        [Required]
        [Range(typeof(TimeSpan), "00:00:30", "1.00:00:00", ErrorMessage = "CodeLifetime must be between 30 seconds and 1 day.")]
        public required TimeSpan CodeLifetime
        {
            get;
            init;
        }

        /// <summary>
        /// Number of backup codes generated for account recovery.
        /// </summary>
        [Required]
        [Range(1, 100, ErrorMessage = "BackupCodeCount must be at least 1.")]
        public required int BackupCodeCount
        {
            get;
            init;
        }

        /// <summary>
        /// Length of each backup code.
        /// </summary>
        [Required]
        [Range(6, 64, ErrorMessage = "BackupCodeLength must be at least 6 characters.")]
        public required int BackupCodeLength
        {
            get;
            init;
        }

        /// <summary>
        /// Master secret key used as a base for generating user-specific 2FA secrets.
        /// </summary>
        [Required]
        [StringLength(44, MinimumLength = 44, ErrorMessage = "MasterKey must be exactly 32 bytes.")]
        public required string MasterKey
        {
            get;
            init;
        }
    }
}
