using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models.Requests
{
    public class LoginRequestModel
    {
        public LoginRequestModel(string login, string password)
        {
            this.Login = login;
            this.Password = password;
            this.Identifiers = App.Services!.GetRequiredService<SystemIdentifierModel>();
        }

        public string Login { get; set; }
        public string Password { get; set; }
        public SystemIdentifierModel Identifiers { get; set; }
    }
}
