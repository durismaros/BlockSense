namespace BlockSenseAPI.Models.User
{
    public class AdditionalUserInfoModel
    {
        public int InvitedUsers { get; set; }
        public int ActiveDevices { get; set; }
        public bool TwoFaEnabled { get; set; }
    }
}
