namespace BlockSense.Contracts.DTOs.Wallet
{
    public sealed record NextNonceResponse
    {
        public required long NextAvailableNonce
        {
            get;
            init;
        }
    }
}
