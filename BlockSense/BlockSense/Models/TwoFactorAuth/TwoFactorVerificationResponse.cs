namespace BlockSenseAPI.Models.TwoFactorAuth
{
    public class TwoFactorVerificationResponse
    {
        public bool Verification {  get; set; }
        public string? Message { get; set; }
    }
}
