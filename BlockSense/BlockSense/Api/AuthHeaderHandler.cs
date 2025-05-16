using BlockSense.Client.Token_authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Api
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly AccessTokenManager _accessTokenManager;

        public AuthHeaderHandler(AccessTokenManager accessTokenManager)
        {
            _accessTokenManager = accessTokenManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri is null)
                throw new Exception("Url cannot be empty");

            if (request.RequestUri.PathAndQuery.Contains("/auth/otp-enable"))
            {
                var token = _accessTokenManager.RetrieveToken();
                Console.WriteLine(token);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Skip adding auth header for public endpoints (e.g., Login, Register)
            if (!request.RequestUri.PathAndQuery.Contains("/auth/"))
            {
                var token = _accessTokenManager.RetrieveToken();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
