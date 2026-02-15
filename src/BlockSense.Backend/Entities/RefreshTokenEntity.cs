namespace BlockSense.Backend.Entities
{
    public sealed class RefreshTokenEntity
    {
        public required string TokenHash
        {
            get;
            set;
        }

        public required uint UserId
        {
            get;
            set;
        }

        public required string IpAddress
        {
            get;
            set;
        }

        public required string DeviceIdentifier
        {
            get;
            set;
        }

        public required string DeviceOs
        {
            get;
            set;
        }

        public required string HardwareFingerprint
        {
            get;
            set;
        }

        public required string NetworkFingerprint
        {
            get;
            set;
        }

        public required DateTime IssuedAt
        {
            get;
            set;
        }

        public required DateTime ExpiresAt
        {
            get;
            set;
        }

        public required bool IsRevoked
        {
            get;
            set;
        }

        public bool IsActive
            => !IsRevoked && ExpiresAt > DateTime.UtcNow;

        public TimeSpan TimeUntilExpiration
            => ExpiresAt - DateTime.UtcNow;
    }
}
