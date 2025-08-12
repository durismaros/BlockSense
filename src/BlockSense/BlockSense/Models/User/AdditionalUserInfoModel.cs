using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models.User
{
    public class AdditionalUserInfoModel
    {
        public int InvitedUsers { get; set; }
        public int ActiveDevices { get; set; }
        public bool TwoFaEnabled { get; set; }
    }
}
