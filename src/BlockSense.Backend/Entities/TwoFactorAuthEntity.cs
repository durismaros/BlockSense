namespace BlockSense.Backend.Entities
{
    public sealed class TwoFactorAuthEntity
    {
        public required uint UserId
        {
            get;
            set;
        }

        public required byte[] EncryptedTotpSecret
        {
            get;
            set;
        }

        public string? BackupCodes
        {
            get;
            set;
        }

        public required DateTime UpdatedAt
        {
            get;
            set;
        }
    }
}
