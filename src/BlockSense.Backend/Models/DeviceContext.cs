using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Models
{
    public sealed record DeviceContext
    {
        /// <summary>
        /// The public IP address of the client.
        /// </summary>
        public required string IpAddress { get; init; }

        /// <summary>
        /// A unique device identifier (e.g., hardware ID or client-generated GUID).
        /// </summary>
        public required string DeviceIdentifier { get; init; }

        /// <summary>
        /// Hardware fingerprint derived from CPU, GPU, and other system info.
        /// </summary>
        public required string HardwareFingerprint { get; init; }

        /// <summary>
        /// Network fingerprint derived from MAC, network stack, or other unique identifiers.
        /// </summary>
        public required string NetworkFingerprint { get; init; }

        /// <summary>
        /// The operating system or platform of the client device.
        /// </summary>
        public required string DeviceOs { get; init; }

        /// <summary>
        /// Factory method to create a DeviceContext from HttpContext headers.
        /// </summary>
        public static DeviceContext FromHttpContext(HttpContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            return new DeviceContext
            {
                DeviceIdentifier = GetHeader(DeviceHeaders.DeviceId),
                HardwareFingerprint = GetHeader(DeviceHeaders.HardwareFingerprint),
                NetworkFingerprint = GetHeader(DeviceHeaders.NetworkFingerprint),
                DeviceOs = GetHeader(DeviceHeaders.DeviceOs),
                IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
            };

            string GetHeader(string key)
            {
                if (!context.Request.Headers.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidDeviceContextException(key);
                }

                return value.ToString();
            }
        }
    }
}
