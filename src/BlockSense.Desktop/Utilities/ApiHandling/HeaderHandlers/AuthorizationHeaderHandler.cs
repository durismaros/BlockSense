using BlockSense.Contracts.Definitions;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Utilities.ApiHandling.HeaderHandlers
{
    public sealed class AuthorizationHeaderHandler : DelegatingHandler
    {
        private static readonly HttpRequestOptionsKey<bool> AddBearerTokenKey =
            new(nameof(ApiRequestOptions.AddBearerToken));

        private readonly ILogger<AuthorizationHeaderHandler> _logger;
        private readonly IServiceProvider _serviceProvider;

        public AuthorizationHeaderHandler(ILogger<AuthorizationHeaderHandler> logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Options.TryGetValue(AddBearerTokenKey, out var addBearerToken) && addBearerToken)
            {
                var tokenProvider =
                    _serviceProvider.GetRequiredService<IAccessTokenProvider>();

                var accessToken = tokenProvider.Get();

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
