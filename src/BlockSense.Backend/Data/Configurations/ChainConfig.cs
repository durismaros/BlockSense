using System.ComponentModel.DataAnnotations;

namespace BlockSense.Backend.Data.Configurations
{
    public sealed record ChainConfig
    {
        [Required]
        public required string Network
        {
            get;
            init;
        }
    }
}
