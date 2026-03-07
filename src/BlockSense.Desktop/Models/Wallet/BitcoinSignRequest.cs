namespace BlockSense.Desktop.Models.Wallet
{
    public sealed record BitcoinSignRequest
    {
        public required byte[] Seed { get; init; }

        public required string ToAddress { get; init; }

        public required decimal AmountBtc { get; init; }

        public required decimal FeeBtc { get; init; }

        public required decimal BalanceBtc { get; init; }
    }
}
