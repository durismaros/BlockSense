using System.ComponentModel.DataAnnotations;

namespace BlockSense.Backend.Data.Configurations
{
    public sealed record CryptoConfig
    {
        [Required]
        public required string ApiKey
        {
            get;
            init;
        }

        [Required]
        public required string BaseUrl
        {
            get;
            init;
        }

        [Required]
        [Range(typeof(TimeSpan), "00:05:00", "01:00:00", ErrorMessage = "Exchange cache duration must be between 1 minute and 1 hour.")]
        public required TimeSpan ExchangeCacheDuration
        {
            get;
            init;
        }

        [Required]
        public required ChainConfig Bitcoin
        {
            get;
            init;
        }

        [Required]
        public required ChainConfig Ethereum
        {
            get;
            init;
        }
    }
}
