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
        public required List<BtcTxParty> Senders { get; set; }
        public required List<BtcTxParty> Recipients { get; set; }
        public required long Timestamp { get; set; }
    }

    internal sealed class BtcTxParty
    {
        public required string Address { get; set; }
        public required AmountValue Value { get; set; }
    }
}
