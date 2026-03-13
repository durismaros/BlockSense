namespace BlockSense.Contracts.DTOs.Transaction
{
    /// <summary>
    /// Represents an unspent transaction output (UTXO).
    /// </summary>
    public sealed record UtxoDto
    {
        /// <summary>
        /// The unique identifier of the transaction that produced this output.
        /// </summary>
        public required string TransactionId
        {
            get;
            init;
        }

        /// <summary>
        /// The zero-based index of this output within its transaction.
        /// </summary>
        public required int OutputIndex
        {
            get;
            init;
        }

        /// <summary>
        /// The value of this unspent output.
        /// </summary>
        public required decimal Amount
        {
            get;
            init;
        }
    }
}