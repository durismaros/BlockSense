using BlockSenseAPI.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSenseAPI.Models.Login
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public RefreshToken? RefreshToken { get; set; }
        public AccessToken? AccessToken { get; set; }
        public bool TwoFactorRequired {  get; set; }
    }
}
