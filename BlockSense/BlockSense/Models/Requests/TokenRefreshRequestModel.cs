using BlockSense.Models.Token;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models.Requests
{
    public class TokenRefreshRequestModel
    {
        public TokenRefreshRequestModel(RefreshTokenModel clientRefreshToken, SystemIdentifierModel clientIdentifiers)
        {
            this.RefreshToken = clientRefreshToken;
            this.Identifiers = clientIdentifiers;
        }

        public RefreshTokenModel RefreshToken { get; set; }
        public SystemIdentifierModel Identifiers { get; set; }
    }
}
