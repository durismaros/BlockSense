using System;

namespace BlockSenseAPI.Models
{
    public class UserInfo
    {
        public int Uid { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public UserType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string InvitingUser { get; set; }

        public class AdditionalInformation
        {
            public int InvitedUsers { get; set; }
            public int ActiveDevices { get; set; }
        }
    }

    public enum UserType
    {
        User,
        Admin,
        Banned
    }
}
