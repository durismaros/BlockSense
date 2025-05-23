using BlockSense.Client.Token_authentication;
using BlockSense.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            try
            {
                // Skip adding auth header for public endpoints
                if (IsPublicEndpoint(request.RequestUri))
                {
                    return await base.SendAsync(request, cancellationToken);
                }

                // Add auth header for protected endpoints
                var token = _accessTokenManager.RetrieveToken();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                return await base.SendAsync(request, cancellationToken);
            }
            catch
            {
                ConsoleLogger.Log($"Failed to process request to {request.RequestUri}");
                return CreateErrorResponse(request, HttpStatusCode.ServiceUnavailable, "Service temporarily unavailable");
            }
        }

        private bool IsPublicEndpoint(Uri? requestUri)
        {
            if (requestUri is null) return true;

            var path = requestUri.PathAndQuery.ToLowerInvariant();
            return path.Contains("/auth/") || path.Contains("/status/");
        }

        private HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, string message)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                RequestMessage = request,
                Content = new StringContent(message)
            };
            return response;
        }
    }
}
