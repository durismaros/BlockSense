namespace BlockSense.Contracts.DTOs.Transaction
{
    public sealed record BroadcastTransactionResponse
    {
        public required string TransactionId
        {
            get;
            init;
        }
    }
}
