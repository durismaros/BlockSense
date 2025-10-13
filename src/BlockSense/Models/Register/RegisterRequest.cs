using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models.Register
{
    public class RegisterRequest
    {
        public RegisterRequest(string username, string email, string password, string invitationCode)
        {
            Username = username;
            Email = email;
            Password = password;
            InvitationCode = invitationCode;
        }

        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string InvitationCode { get; set; }
    }
}
