namespace BlockSense.Backend.Models.Crypto
{
    /// <summary>Represents the API envelope for a next-available-nonce response.</summary>
    internal sealed class NextNonceEnvelope
    {
        public required NextNonceData Data { get; set; }
    }

    /// <summary>Wraps the nonce data item returned by the API.</summary>
    internal sealed class NextNonceData
    {
        public required NextNonceItem Item { get; set; }
    }

    /// <summary>Contains the next available nonce value for an address.</summary>
    internal sealed class NextNonceItem
    {
        public required long NextAvailableNonce { get; set; }
    }
}