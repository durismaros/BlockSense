namespace BlockSense.Contracts.DTOs.Wallet
{
    public sealed record WalletBalanceResponse
    {
        public required string Address
        {
            get;
            init;
        }

        public required decimal Balance
        {
            get;
            init;
        }

        public required string Currency
        {
            get;
            init;
        }
    }
}
