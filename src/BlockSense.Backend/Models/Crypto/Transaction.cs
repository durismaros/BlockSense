namespace BlockSense.Backend.Models.Crypto
{
    internal sealed class BroadcastEnvelope
    {
        public required BroadcastData Data { get; set; }
    }
    internal sealed class BroadcastData
    {
        public required BroadcastItem Item { get; set; }
    }
    internal sealed class BroadcastItem
    {
        public required string TransactionId { get; set; }
    }
}
