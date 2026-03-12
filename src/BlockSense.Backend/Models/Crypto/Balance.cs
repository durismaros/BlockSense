namespace BlockSense.Backend.Models.Crypto
{
    /// <summary>Represents the API envelope for a wallet balance response.</summary>
    internal sealed class BalanceEnvelope
    {
        public required BalanceData Data { get; set; }
    }

    /// <summary>Wraps the balance data item returned by the API.</summary>
    internal sealed class BalanceData
    {
        public required BalanceItem Item { get; set; }
    }

    /// <summary>Contains the confirmed balance for a wallet address.</summary>
    internal sealed class BalanceItem
    {
        public required AmountValue ConfirmedBalance { get; set; }
    }

    /// <summary>Represents an amount and its unit (e.g., BTC, ETH).</summary>
    internal sealed class AmountValue
    {
        public required string Amount { get; set; }
        public required string Unit { get; set; }
    }
}