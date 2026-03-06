using BlockSense.Contracts.Enums;

namespace BlockSense.Contracts.DTOs.Transaction
{
    public sealed record TransactionDto
    {
        public required string TxHash
        {
            get;
            init;
        }

        public required decimal Fee
        {
            get;
            init;
        }

        public required string FromAddress
        {
            get;
            init;
        }

        public required string ToAddress
        {
            get;
            init;
        }

        public required decimal Amount
        {
            get;
            init;
        }

        public required string Currency
        {
            get;
            init;
        }

        public required TransactionStatus Status
        {
            get;
            init;
        }

        public required DateTime Timestamp
        {
            get;
            init;
        }
    }
}
