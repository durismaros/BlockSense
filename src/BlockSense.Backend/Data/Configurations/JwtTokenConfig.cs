using System.ComponentModel.DataAnnotations;

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
        [Required]
        public required string Issuer
        {
            get;
            init;
        }

        /// <summary>
        /// The audience that the token is intended for (client URL).
        /// </summary>
        [Required]
        public required string Audience
        {
            get;
            init;
        }

        /// <summary>
        /// The duration for which the token remains valid.
        /// </summary>
        [Required]
        [Range(typeof(TimeSpan), "00:01:00", "01:00:00", ErrorMessage = "Expiration must be between 1 minute and 1 hour.")]
        public required TimeSpan Expiration
        {
            get;
            init;
        }

        /// <summary>
        /// The secret key used to sign the JWT token.
        /// </summary>
        [Required]
        [StringLength(172, MinimumLength = 172, ErrorMessage = "SigningKey must be exactly 128 bytes.")]
        public required string SigningKey
        {
            get;
            init;
        }
    }
}
