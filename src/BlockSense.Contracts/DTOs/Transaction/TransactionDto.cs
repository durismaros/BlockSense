using BlockSense.Contracts.Enums;

namespace BlockSense.Contracts.DTOs.Transaction
{
    /// <summary>
    /// Represents a blockchain transaction and its associated metadata.
    /// </summary>
    public sealed record TransactionDto
    {
        /// <summary>
        /// The unique transaction hash identifying this transaction on the blockchain.
        /// </summary>
        public required string TxHash
        {
            get;
            init;
        }

        /// <summary>
        /// The transaction fee charged for processing.
        /// </summary>
        public required decimal Fee
        {
            get;
            init;
        }

        /// <summary>
        /// The sender's blockchain address.
        /// </summary>
        public required string FromAddress
        {
            get;
            init;
        }

        /// <summary>
        /// The recipient's blockchain address.
        /// </summary>
        public required string ToAddress
        {
            get;
            init;
        }

        /// <summary>
        /// The amount transferred in this transaction.
        /// </summary>
        public required decimal Amount
        {
            get;
            init;
        }

        /// <summary>
        /// The currency or asset symbol used in this transaction.
        /// </summary>
        public required string Currency
        {
            get;
            init;
        }

        /// <summary>
        /// The current status of the transaction.
        /// </summary>
        public required TransactionStatus Status
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC timestamp when the transaction was recorded.
        /// </summary>
        public required DateTime Timestamp
        {
            get;
            init;
        }
    }
}