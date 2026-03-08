namespace BlockSense.Backend.Models.Crypto
{
    internal sealed class EthTxListEnvelope
    {
        public required EthTxListData Data { get; set; }
    }

    internal sealed class EthTxListData
    {
        public required List<EthTxItem> Items { get; set; }
    }

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
