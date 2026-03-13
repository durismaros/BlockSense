namespace BlockSense.Desktop.Models.Wallet
{
    public sealed record EthereumSignRequest
    {
        public required byte[] Seed { get; init; }

        public required string ToAddress { get; init; }

        public required decimal AmountEth { get; init; }

        public required decimal GasPriceGwei { get; init; }

        public required long GasLimit { get; init; }

        public required long Nonce { get; init; }

        public required long ChainId { get; init; }
    }
}
