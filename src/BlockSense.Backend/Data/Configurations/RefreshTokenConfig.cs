using System.ComponentModel.DataAnnotations;

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
        [Required]
        [Range(typeof(TimeSpan), "00:01:00", "365.00:00:00", ErrorMessage = "Expiration must be between 1 minute and 365 days.")]
        public required TimeSpan Expiration
        {
            get;
            init;
        }
    }
}
