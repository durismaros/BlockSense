using System.Text.Json;

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

        public required string? BackupCodes
        {
            get;
            set;
        }

        public IList<string> BackupCodesList
        {
            get
            {
                return string.IsNullOrEmpty(BackupCodes)
                    ? []
                    : JsonSerializer.Deserialize<List<string>>(BackupCodes) ?? [];
            }
            set
            {
                BackupCodes = value is null
                    ? null
                    : JsonSerializer.Serialize(value);
            }
        }

        public required DateTime CreatedAt
        {
            get;
            init;
        }

        public required DateTime UpdatedAt
        {
            get;
            set;
        }

        public bool HasBackupCodes
            => BackupCodesList.Count > 0;
    }
}
