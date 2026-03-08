namespace BlockSense.Contracts.DTOs.Transaction
{
    public sealed record UtxoDto
    {
        public required string TransactionId { get; set; }
        public required int OutputIndex { get; set; }
        public required decimal Amount { get; set; }
    }
}
