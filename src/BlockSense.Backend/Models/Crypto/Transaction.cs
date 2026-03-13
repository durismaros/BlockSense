namespace BlockSense.Backend.Models.Crypto
{
    /// <summary>Represents the API envelope for a broadcast transaction response.</summary>
    internal sealed class BroadcastEnvelope
    {
        public required BroadcastData Data { get; set; }
    }

    /// <summary>Wraps the broadcast result item returned by the API.</summary>
    internal sealed class BroadcastData
    {
        public required BroadcastItem Item { get; set; }
    }

    /// <summary>Contains the transaction ID resulting from a successful broadcast.</summary>
    internal sealed class BroadcastItem
    {
        public required string TransactionId { get; set; }
    }
}