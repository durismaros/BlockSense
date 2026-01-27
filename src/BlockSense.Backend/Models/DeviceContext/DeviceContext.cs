using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Models.DeviceContext
{
    /// <summary>
    /// Represents the context of a client device making a request to the backend.
    /// </summary>
    public sealed record DeviceContext
    {
        /// <summary>
        /// The public IP address of the client.
        /// </summary>
        public required string IpAddress
        {
            get;
            init;
        }

        /// <summary>
        /// A unique device identifier (e.g., hardware ID or client-generated GUID).
        /// </summary>
        public required string DeviceIdentifier
        {
            get;
            init;
        }

        /// <summary>
        /// The operating system or platform of the client device.
        /// </summary>
        public required string DeviceOs
        {
            get;
            init;
        }

        /// <summary>
        /// Hardware fingerprint derived from CPU, GPU, and other system info.
        /// </summary>
        public required string HardwareFingerprint
        {
            get;
            init;
        }

        /// <summary>
        /// Network fingerprint derived from MAC, network stack, or other unique identifiers.
        /// </summary>
        public required string NetworkFingerprint
        {
            get;
            init;
        }

        /// <summary>
        /// Creates a <see cref="DeviceContext"/> from an <see cref="HttpContext"/>, extracting device headers and IP address.
        /// </summary>
        /// <param name="httpContext">The HTTP context containing request headers and connection information.</param>
        /// <returns>A <see cref="DeviceContext"/> populated from HTTP headers.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpContext"/> is null.</exception>
        /// <exception cref="InvalidDeviceContextException">Thrown if any required device header is missing or empty.</exception>
        public static DeviceContext FromHttpContext(HttpContext httpContext)
        {
            if (httpContext is null)
                throw new ArgumentNullException(nameof(httpContext));

            // Helper to retrieve header values
            string GetHeader(string key)
                => httpContext.Request.Headers.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value.ToString() : throw new InvalidClientContextException();

            return new DeviceContext
            {
                DeviceIdentifier = GetHeader(DeviceHeaders.DeviceIdentifier),
                DeviceOs = GetHeader(DeviceHeaders.DeviceOs),
                HardwareFingerprint = GetHeader(DeviceHeaders.HardwareFingerprint),
                NetworkFingerprint = GetHeader(DeviceHeaders.NetworkFingerprint),
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            };
        }
    }
}
