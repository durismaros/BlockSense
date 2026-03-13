namespace BlockSense.Contracts.DTOs.Transaction
{
    /// <summary>
    /// Represents the transaction history and unspent outputs associated with a blockchain address.
    /// </summary>
    public sealed record TransactionListResponse
    {
        /// <summary>
        /// The blockchain address for which the transaction history was retrieved.
        /// </summary>
        public required string Address
        {
            get;
            init;
        }

        /// <summary>
        /// The total number of transactions associated with the address.
        /// </summary>
        public required int Total
        {
            get;
            init;
        }

        /// <summary>
        /// The list of transactions associated with the address.
        /// </summary>
        public required IEnumerable<TransactionDto> Transactions
        {
            get;
            init;
        }
    }
}