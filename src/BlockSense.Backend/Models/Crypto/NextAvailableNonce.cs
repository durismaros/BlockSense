namespace BlockSense.Backend.Models.Crypto
{
    internal sealed class NextNonceEnvelope
    {
        public required NextNonceData Data { get; set; }
    }
    internal sealed class NextNonceData
    {
        public required NextNonceItem Item { get; set; }
    }
    internal sealed class NextNonceItem
    {
        public required long NextAvailableNonce { get; set; }
    }
}
