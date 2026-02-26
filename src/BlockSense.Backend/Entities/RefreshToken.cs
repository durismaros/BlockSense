namespace BlockSense.Backend.Entities
{
    public sealed class RefreshToken
    {
        public required string TokenHash
        {
            get;
            init;
        }

        public required uint UserId
        {
            get;
            init;
        }

        public required string IpAddress
        {
            get;
            init;
        }

        public required string DeviceIdentifier
        {
            get;
            init;
        }

        public required string DeviceOs
        {
            get;
            init;
        }

        public required string HardwareFingerprint
        {
            get;
            init;
        }

        public required string NetworkFingerprint
        {
            get;
            init;
        }

        public required DateTime IssuedAt
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

        public bool IsExpired
            => DateTime.UtcNow >= ExpiresAt;

        public bool IsValid
            => !IsRevoked && !IsExpired;

        public TimeSpan TimeUntilExpiration
            => ExpiresAt - DateTime.UtcNow;
    }
}
