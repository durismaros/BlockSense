using BlockSense.Models.Token;
using BlockSense.Models.User;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models.Token.DTO
{
    public class TokenRefreshRequest
    {
        public TokenRefreshRequest(RefreshToken clientRefreshToken, SystemIdentifier clientIdentifiers)
        {
            RefreshToken = clientRefreshToken;
            Identifiers = clientIdentifiers;
        }

        public RefreshToken RefreshToken { get; set; }
        public SystemIdentifier Identifiers { get; set; }
    }
}
