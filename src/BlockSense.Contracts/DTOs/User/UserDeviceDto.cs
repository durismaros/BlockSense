using BlockSense.Contracts.Enums.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Contracts.DTOs.User
{
    /// <summary>
    /// Represents a device session associated with a user account.
    /// </summary>
    public sealed record UserDeviceDto
    {
        /// <summary>
        /// Unique identifier of the device session (Hashed value of the refresh token).
        /// </summary>
        public required string TokenHash
        {
            get;
            init;
        }

        /// <summary>
        /// Current status of the device session.
        /// </summary>
        public required UserDeviceStatus Status
        {
            get;
            init;
        }

        /// <summary>
        /// Name or label of the device associated with the token.
        /// </summary>
        public required string DeviceName
        {
            get;
            init;
        }

        /// <summary>
        /// IP address from which the token was initiated.
        /// </summary>
        public required string IpAddress
        {
            get;
            init;
        }

        /// <summary>
        /// UTC timestamp when the token was issued.
        /// </summary>
        public required DateTime IssuedAt
        {
            get;
            init;
        }

        /// <summary>
        /// UTC timestamp when the token expires.
        /// </summary>
        public required DateTime ExpiresAt
        {
            get;
            init;
        }
    }
}
