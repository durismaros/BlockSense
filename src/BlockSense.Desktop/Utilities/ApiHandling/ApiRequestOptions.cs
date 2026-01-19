using System.Net.Http;

namespace BlockSense.Desktop.Utilities.ApiHandling
{
    internal sealed record ApiRequestOptions
    {
        internal bool AddBearerToken
        {
            get;
            init;
        }

        internal bool AddDeviceHeaders
        {
            get;
            init;
        }

        internal void ApplyTo(HttpRequestMessage httpRequest)
        {
            if (AddBearerToken)
            {
                httpRequest.Options.Set(
                    new HttpRequestOptionsKey<bool>(nameof(AddBearerToken)),
                    true);
            }

            if (AddDeviceHeaders)
            {
                httpRequest.Options.Set(
                    new HttpRequestOptionsKey<bool>(nameof(AddDeviceHeaders)),
                    true);
            }
        }
    }
}
