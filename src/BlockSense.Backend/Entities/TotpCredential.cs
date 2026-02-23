namespace BlockSense.Backend.Entities
{
    public sealed class TotpCredential
    {
        public required uint UserId
        {
            get;
            init;
        }

        public required byte[] EncryptedSecret
        {
            get;
            set;
        }

        public string? BackupCodes
        {
            get;
            set;
        }

        public DateTime CreatedAt
        {
            get;
            init;
        }

        public DateTime UpdatedAt
        {
            get;
            set;
        }

        public bool HasBackupCodes
            => !string.IsNullOrWhiteSpace(BackupCodes) && BackupCodes.Trim() != "[]";
    }
}
