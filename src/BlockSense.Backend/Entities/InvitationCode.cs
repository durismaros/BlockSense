namespace BlockSense.Backend.Entities
{
    public sealed class InvitationCode
    {
        public required uint Id
        {
            get;
            init;
        }

        public required string Code
        {
            get;
            init;
        }

        public required uint IssuedToId
        {
            get;
            init;
        }

        public required uint? RedeemedById
        {
            get;
            set;
        }

        public string? RedeemedByUsername
        {
            get;
            set;
        }

        public required DateTime CreatedAt
        {
            get;
            init;
        }

        public required DateTime ExpiresAt
        {
            get;
            init;
        }

        public required bool IsRevoked
        {
            get;
            set;
        }

        public bool IsRedeemed
            => RedeemedById is not null;

        public bool IsExpired
            => DateTime.UtcNow >= ExpiresAt;

        public bool IsValid
            => !IsRevoked && !IsRedeemed && DateTime.UtcNow < ExpiresAt;

        public TimeSpan TimeUntilExpiration
            => ExpiresAt - DateTime.UtcNow;
    }
}
