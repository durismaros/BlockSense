namespace BlockSense.Contracts.Enums
{
    /// <summary>
    /// Classifies what kind of actor initiated an activity log event.
    /// </summary>
    /// <remarks>
    /// The string values must match the database <c>activity_logs.type</c>
    /// ENUM exactly. Every log entry is always attributed to a specific user
    /// via <c>ActivityLog.UserId</c> (non-nullable). This enum further
    /// describes whether the action was taken directly by the user, triggered
    /// by an internal system process acting on their behalf, or executed by
    /// a scheduled job in the context of that user.
    /// </remarks>
    public enum ActivityType
    {
        /// <summary>
        /// The user directly initiated the action via an API request or UI interaction.
        /// </summary>
        User = 0,

        /// <summary>
        /// An internal application process performed the action on behalf of the user
        /// (e.g. a background service, middleware pipeline, or event handler).
        /// </summary>
        System = 1,

        /// <summary>
        /// A scheduled background job performed the action in the context of the user
        /// (e.g. a Hangfire job, hosted service timer, or OS cron task).
        /// </summary>
        Cron = 2,
    }
}
