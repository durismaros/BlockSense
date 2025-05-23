namespace BlockSenseAPI.Models
{
    public class InviteInfoModel
    {
        public string? InvitationCode { get; set; }
        public string? CreationDate { get; set; }
        public string? ExpirationDate { get; set; }
        public string? InvitedUser { get; set; }
        public bool IsUsed { get; set; }
        public string? Status { get; set; }
    }
}
