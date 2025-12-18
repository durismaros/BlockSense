using BlockSense.Contracts.Enums;

namespace BlockSense.Contracts.DTOs.Utilities
{
    /// <summary>
    /// Represents the current status of the system, including the server and database.
    /// </summary>
    public sealed record SystemHealthStatus
    {
        /// <summary>
        /// The timestamp of the status report in Coordinated Universal Time (UTC).
        /// </summary>
        public DateTimeOffset TimeStamp { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Current operational status of the server.
        /// </summary>
        public ServiceStatus ServerStatus { get; init; } = ServiceStatus.Unknown;

        /// <summary>
        /// Current operational status of the database.
        /// </summary>
        public ServiceStatus DatabaseStatus { get; init; } = ServiceStatus.Unknown;

        /// <summary>
        /// Optional message providing additional details about the server or database status.
        /// </summary>
        public string? Message { get; init; }
    }
}
