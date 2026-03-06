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

        /// <summary>Crypto APIs REST base URL (default: https://rest.cryptoapis.io).</summary>
        [Required]
        public required string BaseUrl
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
