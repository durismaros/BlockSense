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

        public required uint GeneratedBy
        {
            get;
            init;
        }

        public uint? UsedBy
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

        public string? UsedByUsername
        {
            get;
            set;
        }

        public bool IsActive
            => !IsRevoked && UsedBy is null && DateTime.UtcNow < ExpiresAt;

        public TimeSpan TimeUntilExpiration
            => ExpiresAt - DateTime.UtcNow;
    }
}
