using BlockSense.Server.Cryptography.TokenAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models.Responses
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public TokenCache TokenData { get; set; }
    }

    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
