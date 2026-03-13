namespace BlockSense.Contracts.Enums
{
    /// <summary>
    /// Represents the processing status of a blockchain transaction.
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>
        /// The transaction has been submitted but not yet confirmed by the network.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The transaction has been confirmed and recorded on the blockchain.
        /// </summary>
        Confirmed = 1,

        /// <summary>
        /// The transaction was rejected or could not be processed by the network.
        /// </summary>
        Failed = 2
    }
}