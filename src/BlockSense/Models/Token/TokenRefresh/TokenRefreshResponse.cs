using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models.Token.DTO
{
    public class TokenRefreshResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public AccessToken? AccessToken { get; set; }
    }
}
