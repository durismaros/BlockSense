namespace BlockSense.Desktop.Models.Wallet
{
    public sealed record WalletCreationContext
    {
        public required string Mnemonic
        {
            get;
            init;
        }
        public required bool IsImport
        {
            get;
            init;
        }
    }
}
