using BlockSense.Models.User;
using Microsoft.Extensions.DependencyInjection;

namespace BlockSense.Models.Login
{
    public class LoginRequest
    {
        public LoginRequest(string login, string password)
        {
            Login = login;
            Password = password;
            Identifiers = App.Services!.GetRequiredService<SystemIdentifier>();
        }

        public LoginRequest(string login, string password, string code)
        {
            Login = login;
            Password = password;
            Identifiers = App.Services!.GetRequiredService<SystemIdentifier>();
            TwoFaCode = code;
        }

        public string Login { get; set; }
        public string Password { get; set; }
        public SystemIdentifier Identifiers { get; set; }
        public string? TwoFaCode { get; set; }
    }
}
