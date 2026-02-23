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
        [Required]
        [StringLength(64, MinimumLength = 3)]
        [RegularExpression(@"^[A-Za-z0-9\-]+$")]
        public required string DeviceIdentifier
        {
            get;
            init;
        }

        [Required]
        [StringLength(64, MinimumLength = 3)]
        [RegularExpression(@"^[A-Za-z0-9\s\.\-_]+$")]
        public required string DeviceOs
        {
            get;
            init;
        }

        [Required]
        [StringLength(44, MinimumLength = 44)]
        [RegularExpression(@"^[A-Za-z0-9+/]{43}=$")]
        public required string HardwareFingerprint
        {
            get;
            init;
        }

        [Required]
        [StringLength(17, MinimumLength = 17)]
        [RegularExpression(@"^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$")]
        public required string NetworkFingerprint
        {
            get;
            init;
        }

        [Required]
        public required string IpAddress
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
                => httpContext.Request.Headers.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                    ? value.ToString() : throw new InvalidClientContextException();

            return new DeviceContext
            {
                DeviceIdentifier = GetHeader(DeviceHeaders.DeviceIdentifier),
                DeviceOs = GetHeader(DeviceHeaders.DeviceOs),
                HardwareFingerprint = GetHeader(DeviceHeaders.HardwareFingerprint),
                NetworkFingerprint = GetHeader(DeviceHeaders.NetworkFingerprint),
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? throw new InvalidClientContextException()
            };
        }
    }
}
