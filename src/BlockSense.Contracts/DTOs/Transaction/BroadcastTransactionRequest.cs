using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Transaction
{
    public sealed record BroadcastTransactionRequest
    {
        [Required]
        public required string SignedTransactionHex
        {
            get;
            init;
        }
    }
}
