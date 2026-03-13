namespace BlockSense.Contracts.DTOs.Wallet
{
    /// <summary>
    /// Represents the balance of a wallet address for a specific asset.
    /// </summary>
    public sealed record WalletBalanceResponse
    {
        /// <summary>
        /// The blockchain address of the wallet.
        /// </summary>
        public required string Address
        {
            get;
            init;
        }

        /// <summary>
        /// The current balance of the wallet.
        /// </summary>
        public required decimal Balance
        {
            get;
            init;
        }

        /// <summary>
        /// The currency or asset symbol for the reported balance.
        /// </summary>
        public required string Currency
        {
            get;
            init;
        }
    }
}