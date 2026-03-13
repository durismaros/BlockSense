using System.ComponentModel.DataAnnotations;

namespace BlockSense.Backend.Data.Configurations
{
    /// <summary>
    /// Configuration settings for a specific blockchain network.
    /// </summary>
    public sealed record ChainConfig
    {
        /// <summary>
        /// The blockchain network name (e.g., "testnet", "mainnet", "sepolia").
        /// </summary>
        [Required]
        public required string Network
        {
            get;
            init;
        }
    }
}