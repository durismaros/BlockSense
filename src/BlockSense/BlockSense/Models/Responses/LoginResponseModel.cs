using BlockSense.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models.Responses
{
    public class LoginResponseModel
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public RefreshTokenModel? RefreshToken { get; set; }
        public AccessTokenModel? AccessToken { get; set; }
    }
}
