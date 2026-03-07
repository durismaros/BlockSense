namespace BlockSense.Contracts.DTOs.Wallet
{
    public sealed record ExchangeRateResponse
    {
        public required string FromAssetId
        {
            get;
            init;
        }

        public required string FromAssetSymbol
        {
            get;
            init;
        }

        public required decimal Rate
        {
            get;
            init;
        }

        public required string ToAssetId
        {
            get;
            init;
        }

        public required string ToAssetSymbol
        {
            get;
            init;
        }

        public required DateTime CachedAt
        {
            get;
            init;
        }
    }
}
