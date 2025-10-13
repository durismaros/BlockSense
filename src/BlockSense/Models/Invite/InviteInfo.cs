using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models.Invite
{
    public class InviteInfo
    {
        public string? InvitationCode { get; set; }
        public string? CreationDate { get; set; }
        public string? ExpirationDate { get; set; }
        public string? InvitedUser { get; set; }
        public bool IsUsed { get; set; }
        public string? Status { get; set; }
    }
}
