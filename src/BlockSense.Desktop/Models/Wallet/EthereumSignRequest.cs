namespace BlockSense.Desktop.Models.Wallet
{
    public sealed record EthereumSignRequest
    {
        public required byte[] Seed { get; init; }

        public required string ToAddress { get; init; }

        public required decimal AmountEth { get; init; }

        public required decimal GasPriceGwei { get; init; }

        public required ulong GasLimit { get; init; }

        public required ulong Nonce { get; init; }

        public required ulong ChainId { get; init; }
    }
}
