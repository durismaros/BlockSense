namespace BlockSense.Backend.Models.Crypto
{
    /// <summary>Represents the API envelope for an Ethereum transaction list response.</summary>
    internal sealed class EthTxListEnvelope
    {
        public required EthTxListData Data { get; set; }
    }

    /// <summary>Wraps the list of Ethereum transaction items returned by the API.</summary>
    internal sealed class EthTxListData
    {
        public required List<EthTxItem> Items { get; set; }
    }

    /// <summary>Represents a single Ethereum transaction returned by the API.</summary>
    internal sealed class EthTxItem
    {
        public required string Hash { get; set; }
        public required AmountValue Fee { get; set; }
        public required string Sender { get; set; }
        public required string Recipient { get; set; }
        public required string Status { get; set; }
        public required AmountValue Value { get; set; }
        public required long Timestamp { get; set; }
    }
}