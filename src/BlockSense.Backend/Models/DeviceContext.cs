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
    }
}
