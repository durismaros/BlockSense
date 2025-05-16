using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models.Requests
{
    public class RegisterRequestModel
    {
        public RegisterRequestModel(string username, string email, string password, string invitationCode)
        {
            this.Username = username;
            this.Email = email;
            this.Password = password;
            this.InvitationCode = invitationCode;
        }

        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string InvitationCode { get; set; }
    }
}
