namespace BlockSense.Contracts.DTOs.Wallet
{
    /// <summary>
    /// Represents the next available nonce for a wallet address.
    /// </summary>
    public sealed record NextNonceResponse
    {
        /// <summary>
        /// The next nonce value available for submitting a transaction.
        /// </summary>
        public required long NextAvailableNonce
        {
            get;
            init;
        }
    }
}