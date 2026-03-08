namespace BlockSense.Backend.Models.Crypto
{
    internal sealed class BtcTxListEnvelope
    {
        public required BtcTxListData Data { get; set; }
    }

    internal sealed class BtcTxListData
    {
        public required List<BtcTxItem> Items { get; set; }
    }

    internal sealed class BtcTxItem
    {
        public required string Id { get; set; }
        public required string Hash { get; set; }
        public AmountValue? Fee { get; set; }
        public required List<BtcInputItem> Inputs { get; set; }
        public required List<BtcOutputItem> Outputs { get; set; }
        public required long Timestamp { get; set; }
    }

    internal sealed class BtcInputItem
    {
        public List<string>? Addresses { get; set; }
        public required int OutputIndex { get; set; }
        public string? TransactionId { get; set; }
        public AmountValue? Value { get; set; }
    }

    internal sealed class BtcOutputItem
    {
        public List<string>? Addresses { get; set; }
        public required bool IsSpent { get; set; }
        public AmountValue? Value { get; set; }
    }
}
