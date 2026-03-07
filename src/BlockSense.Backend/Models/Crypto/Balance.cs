namespace BlockSense.Backend.Models.Crypto
{
    internal sealed class BalanceEnvelope
    {
        public required BalanceData Data { get; set; }
    }

    internal sealed class BalanceData
    {
        public required BalanceItem Item { get; set; }
    }

    internal sealed class BalanceItem
    {
        public required AmountValue ConfirmedBalance { get; set; }
    }

    internal sealed class AmountValue
    {
        public required string Amount { get; set; }
        public required string Unit { get; set; }
    }
}
