using BlockSense.Desktop.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Utilities.ApiHandling
{
    public sealed class AuthorizationHeaderHandler : DelegatingHandler
    {
        private readonly IServiceProvider _serviceProvider;

        private static readonly HttpRequestOptionsKey<bool> AddBearerTokenKey =
            new(nameof(ApiRequestOptions.AddBearerToken));

        public AuthorizationHeaderHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Options.TryGetValue(AddBearerTokenKey, out var addBearerToken) && addBearerToken)
            {
                var tokenProvider =
                    _serviceProvider.GetRequiredService<IAccessTokenProvider>();

                var accessToken = await tokenProvider.GetAsync(cancellationToken);

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
