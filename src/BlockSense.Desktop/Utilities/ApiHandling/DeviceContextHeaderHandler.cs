using BlockSense.Contracts.Definitions;
using BlockSense.Desktop.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Utilities.ApiHandling
{
    public sealed class DeviceContextHeaderHandler : DelegatingHandler
    {
        private readonly IServiceProvider _serviceProvider;

        private static readonly HttpRequestOptionsKey<bool> AddDeviceHeadersKey =
            new(nameof(ApiRequestOptions.AddDeviceHeaders));

        public DeviceContextHeaderHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Options.TryGetValue(AddDeviceHeadersKey, out var addDeviceHeaders) && addDeviceHeaders)
            {
                var deviceContextProvider =
                    _serviceProvider.GetRequiredService<IDeviceContextProvider>();

                request.Headers.TryAddWithoutValidation(
                    DeviceHeaders.DeviceIdentifier,
                    deviceContextProvider.DeviceIdentifier);

                request.Headers.TryAddWithoutValidation(
                    DeviceHeaders.DeviceOs,
                    deviceContextProvider.DeviceOs);

                request.Headers.TryAddWithoutValidation(
                    DeviceHeaders.HardwareFingerprint,
                    deviceContextProvider.HardwareFingerprint);

                request.Headers.TryAddWithoutValidation(
                    DeviceHeaders.NetworkFingerprint,
                    deviceContextProvider.NetworkFingerprint);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
