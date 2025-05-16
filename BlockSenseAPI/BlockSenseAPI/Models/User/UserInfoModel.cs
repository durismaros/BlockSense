using BlockSenseAPI.Services;

namespace BlockSenseAPI.Models.User
{
    public class UserInfoModel
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public UserType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? InvitingUser { get; set; }
    }

    public enum UserType
    {
        User,
        Admin,
        Banned
    }
}
