namespace BlockSense.Backend.Models.Crypto
{
    /// <summary>Represents the API envelope for a Bitcoin transaction list response.</summary>
    internal sealed class BtcTxListEnvelope
    {
        public required BtcTxListData Data { get; set; }
    }

    /// <summary>Wraps the list of Bitcoin transaction items returned by the API.</summary>
    internal sealed class BtcTxListData
    {
        public required List<BtcTxItem> Items { get; set; }
    }

    /// <summary>Represents a single Bitcoin transaction returned by the API.</summary>
    internal sealed class BtcTxItem
    {
        public required string Id { get; set; }
        public required string Hash { get; set; }
        public AmountValue? Fee { get; set; }
        public required List<BtcInputItem> Inputs { get; set; }
        public required List<BtcOutputItem> Outputs { get; set; }
        public required long Timestamp { get; set; }
    }

    /// <summary>Represents a single input in a Bitcoin transaction.</summary>
    internal sealed class BtcInputItem
    {
        public List<string>? Addresses { get; set; }
        public required int OutputIndex { get; set; }
        public string? TransactionId { get; set; }
        public AmountValue? Value { get; set; }
    }

    /// <summary>Represents a single output in a Bitcoin transaction.</summary>
    internal sealed class BtcOutputItem
    {
        public List<string>? Addresses { get; set; }
        public required bool IsSpent { get; set; }
        public AmountValue? Value { get; set; }
    }
}