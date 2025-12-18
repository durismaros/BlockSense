namespace BlockSense.Contracts.Enums
{
    /// <summary>
    /// Represents the operational status of a service or system component.
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// The service status is unknown or has not been determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// The service is reachable and operating normally.
        /// </summary>
        Online,

        /// <summary>
        /// The service is not reachable or is currently offline.
        /// </summary>
        Offline,

        /// <summary>
        /// The service is intentionally unavailable due to maintenance.
        /// </summary>
        Maintenance
    }
}
