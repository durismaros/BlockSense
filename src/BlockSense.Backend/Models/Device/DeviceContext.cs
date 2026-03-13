using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Contracts.Definitions;
using System.ComponentModel.DataAnnotations;

namespace BlockSense.Backend.Models.Device
{
    /// <summary>
    /// Represents the context of a client device making a request to the backend.
    /// </summary>
    public sealed record DeviceContext
    {
        /// <summary>Gets the unique identifier of the device.</summary>
        [Required]
        [StringLength(64, MinimumLength = 3)]
        [RegularExpression(@"^[A-Za-z0-9\-]+$")]
        public required string DeviceIdentifier { get; init; }

        /// <summary>Gets the operating system of the device.</summary>
        [Required]
        [StringLength(64, MinimumLength = 3)]
        [RegularExpression(@"^[A-Za-z0-9\s\.\-_]+$")]
        public required string DeviceOs { get; init; }

        /// <summary>Gets the Base64-encoded hardware fingerprint of the device.</summary>
        [Required]
        [StringLength(44, MinimumLength = 44)]
        [RegularExpression(@"^[A-Za-z0-9+/]{43}=$")]
        public required string HardwareFingerprint { get; init; }

        /// <summary>Gets the MAC address used as a network fingerprint for the device.</summary>
        [Required]
        [StringLength(17, MinimumLength = 17)]
        [RegularExpression(@"^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$")]
        public required string NetworkFingerprint { get; init; }

        /// <summary>Gets the IP address of the connecting client.</summary>
        [Required]
        public required string IpAddress { get; init; }

        /// <summary>
        /// Creates a <see cref="DeviceContext"/> from an <see cref="HttpContext"/>,
        /// extracting required device headers and the remote IP address.
        /// </summary>
        /// <param name="httpContext">The HTTP context containing request headers and connection information.</param>
        /// <returns>A <see cref="DeviceContext"/> populated from the HTTP request.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpContext"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidClientContextException">Thrown if any required device header is missing or empty.</exception>
        public static DeviceContext FromHttpContext(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            return new DeviceContext
            {
                DeviceIdentifier = GetRequiredHeader(httpContext, DeviceHeaders.DeviceIdentifier),
                DeviceOs = GetRequiredHeader(httpContext, DeviceHeaders.DeviceOs),
                HardwareFingerprint = GetRequiredHeader(httpContext, DeviceHeaders.HardwareFingerprint),
                NetworkFingerprint = GetRequiredHeader(httpContext, DeviceHeaders.NetworkFingerprint),
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString()
                                        ?? throw new InvalidClientContextException()
            };
        }

        private static string GetRequiredHeader(HttpContext httpContext, string headerKey)
        {
            return httpContext.Request.Headers.TryGetValue(headerKey, out var value)
                && !string.IsNullOrWhiteSpace(value)
                    ? value.ToString()
                    : throw new InvalidClientContextException();
        }
    }
}