namespace BlockSense.Contracts.DTOs.Transaction
{
    public sealed record TransactionListResponse
    {
        public required string Address
        {
            get;
            init;
        }

        public required int Total
        {
            get;
            init;
        }

        public required IEnumerable<TransactionDto> Transactions
        {
            get;
            init;
        }

        public required IEnumerable<UtxoDto> Utxos
        {
            get;
            init;
        }
    }
}
