using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Transaction
{
    /// <summary>
    /// Represents a request to broadcast a signed transaction to the network.
    /// </summary>
    public sealed record BroadcastTransactionRequest
    {
        /// <summary>
        /// The fully signed transaction encoded as a hexadecimal string.
        /// </summary>
        [Required(ErrorMessage = "Signed transaction hex is required.")]
        public required string SignedTransactionHex
        {
            get;
            init;
        }
    }
}