namespace BlockSenseAPI.Models.TwoFactorAuth.BackupCode
{
    public class TwoFactorBackupResponse
    {
        public bool Success { get; set; }
        public TwoFactorBackupCodes? BackupCodes { get; set; }
    }
}
