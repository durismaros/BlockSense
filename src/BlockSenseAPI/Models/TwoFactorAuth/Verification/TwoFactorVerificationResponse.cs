namespace BlockSenseAPI.Models.TwoFactorAuth.Verification
{
    public class TwoFactorVerificationResponse
    {
        public bool Verification {  get; set; }
        public string? Message { get; set; }
    }
}
