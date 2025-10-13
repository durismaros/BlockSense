namespace BlockSenseAPI.Models.Invite
{
    public class InvitationDto
    {
        public string? InvitationCode { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? InvitedUser { get; set; }
        public bool IsUsed { get; set; }
        public InvitationStatus? Status { get; set; } // Enum instead of string
    }

    public enum InvitationStatus
    {
        Active,
        Expired,
        Used,
        Revoked
    }
}
