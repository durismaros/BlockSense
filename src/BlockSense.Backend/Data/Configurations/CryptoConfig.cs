using System.ComponentModel.DataAnnotations;

namespace BlockSense.Backend.Data.Configurations
{
    /// <summary>
    /// Configuration settings for the Crypto APIs integration.
    /// </summary>
    public sealed record CryptoConfig
    {
        /// <summary>
        /// The API key used to authenticate requests to the crypto data provider.
        /// </summary>
        [Required]
        public required string ApiKey
        {
            get;
            init;
        }

        /// <summary>
        /// The base URL of the crypto data provider's REST API.
        /// </summary>
        [Required]
        public required string BaseUrl
        {
            get;
            init;
        }

        /// <summary>
        /// The duration for which exchange rate data is cached before being refreshed.
        /// </summary>
        [Required]
        [Range(typeof(TimeSpan), "00:05:00", "01:00:00", ErrorMessage = "ExchangeCacheDuration must be between 5 minutes and 1 hour.")]
        public required TimeSpan ExchangeCacheDuration
        {
            get;
            init;
        }

        /// <summary>
        /// Network configuration for the Bitcoin blockchain.
        /// </summary>
        [Required]
        public required ChainConfig Bitcoin
        {
            get;
            init;
        }

        /// <summary>
        /// Network configuration for the Ethereum blockchain.
        /// </summary>
        [Required]
        public required ChainConfig Ethereum
        {
            get;
            init;
        }
    }
}