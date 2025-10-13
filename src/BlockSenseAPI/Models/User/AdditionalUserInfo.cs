namespace BlockSenseAPI.Models.User
{
    public class AdditionalUserInfo
    {
        public int InvitedUsers { get; set; }
        public int ActiveDevices { get; set; }
        public bool TwoFaEnabled { get; set; }
    }
}
