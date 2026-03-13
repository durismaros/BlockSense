using System.Text.Json;

namespace BlockSense.Backend.Entities
{
    /// <summary>
    /// Represents a user's TOTP (Time-based One-Time Password) credential,
    /// including the encrypted secret and backup codes used for 2FA recovery.
    /// </summary>
    public sealed class TotpCredential
    {
        /// <summary>
        /// The identifier of the user who owns this TOTP credential.
        /// </summary>
        public required uint UserId
        {
            get;
            init;
        }

        /// <summary>
        /// The encrypted TOTP secret used to generate verification codes.
        /// </summary>
        public required byte[] EncryptedSecret
        {
            get;
            set;
        }

        /// <summary>
        /// The JSON-serialized list of hashed backup codes, or <c>null</c> if no backup codes have been generated.
        /// </summary>
        /// <remarks>
        /// Use <see cref="BackupCodesList"/> to read or write backup codes as a typed list.
        /// </remarks>
        public required string? BackupCodes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the backup codes as a typed list, serializing to and from <see cref="BackupCodes"/>.
        /// Returns an empty list when no backup codes are present.
        /// </summary>
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

        /// <summary>
        /// The UTC date and time at which this credential was created.
        /// </summary>
        public required DateTime CreatedAt
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC date and time at which this credential was last updated.
        /// </summary>
        public required DateTime UpdatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether this credential has any backup codes available for account recovery.
        /// </summary>
        public bool HasBackupCodes
            => BackupCodesList.Count > 0;
    }
}