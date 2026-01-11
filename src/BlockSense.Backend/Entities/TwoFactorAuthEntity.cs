using BlockSense.Contracts.Cryptography.Hashing;
using System.Text;
using System.Text.Json;

namespace BlockSense.Backend.Entities
{
    /// <summary>
    /// Represents a user's 2FA (Two-Factor Authentication) configuration stored in the database.
    /// </summary>
    public sealed class TwoFactorAuthEntity
    {
        /// <summary>
        /// The unique identifier of the user this 2FA data belongs to.
        /// </summary>
        public required uint UserId
        {
            get;
            set;
        }

        /// <summary>
        /// The encrypted TOTP (Time-Based One-Time Password) secret.
        /// </summary>
        public required byte[] EncryptedTotpSecret
        {
            get;
            set;
        }

        /// <summary>
        /// An optional array of hashed backup codes used for recovery.
        /// </summary>
        public IReadOnlyList<string>? BackupCodes
        {
            get;
            set;
        }

        /// <summary>
        /// The UTC timestamp when this 2FA data was last updated.
        /// </summary>
        public required DateTime UpdatedAt
        {
            get;
            set;
        }

        /// <summary>
        /// Serializes the <see cref="BackupCodes"/> array to JSON for storage.
        /// </summary>
        public void RemoveBackupCode(string code)
        {
            if (BackupCodes is null)
            {
                return;
            }

            BackupCodes = BackupCodes
                .Where(c => c != Sha256Hasher.ComputeBase64(Encoding.UTF8.GetBytes(code)))
                .ToList();
        }
    }
}
