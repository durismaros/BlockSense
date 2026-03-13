namespace BlockSense.Contracts.DTOs.Wallet
{
    /// <summary>
    /// Represents an exchange rate between two assets.
    /// </summary>
    public sealed record ExchangeRateResponse
    {
        /// <summary>
        /// The unique identifier of the source asset.
        /// </summary>
        public required string FromAssetId
        {
            get;
            init;
        }

        /// <summary>
        /// The ticker symbol of the source asset.
        /// </summary>
        public required string FromAssetSymbol
        {
            get;
            init;
        }

        /// <summary>
        /// The exchange rate from the source asset to the target asset.
        /// </summary>
        public required decimal Rate
        {
            get;
            init;
        }

        /// <summary>
        /// The unique identifier of the target asset.
        /// </summary>
        public required string ToAssetId
        {
            get;
            init;
        }

        /// <summary>
        /// The ticker symbol of the target asset.
        /// </summary>
        public required string ToAssetSymbol
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC timestamp when this exchange rate was last cached.
        /// </summary>
        public required DateTime CachedAt
        {
            get;
            init;
        }
    }
}