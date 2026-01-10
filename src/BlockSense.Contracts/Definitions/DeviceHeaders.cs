namespace BlockSense.Contracts.Definitions
{
    /// <summary>
    /// Defines standardized HTTP headers used to identify and describe client devices.
    /// </summary>
    public static class DeviceHeaders
    {
        /// <summary>
        /// Unique identifier for the device.
        /// </summary>
        public const string DeviceId = "X-Device-Id";

        /// <summary>
        /// Hardware fingerprint of the device.
        /// </summary>
        public const string HardwareFingerprint = "X-Hardware-Fingerprint";

        /// <summary>
        /// Network fingerprint of the device.
        /// </summary>
        public const string NetworkFingerprint = "X-Network-Fingerprint";

        /// <summary>
        /// Operating system of the device.
        /// </summary>
        public const string DeviceOs = "X-Device-OS";
    }
}
