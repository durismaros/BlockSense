namespace BlockSense.Contracts.DTOs.Transaction
{
    /// <summary>
    /// Represents the response returned after successfully broadcasting a transaction.
    /// </summary>
    public sealed record BroadcastTransactionResponse
    {
        /// <summary>
        /// The unique transaction identifier assigned by the network.
        /// </summary>
        public required string TransactionId
        {
            get;
            init;
        }
    }
}