using BlockSense.Backend.Entities;

namespace BlockSense.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Defines data access operations for activity log entries.
    /// </summary>
    public interface IActivityLogRepository
    {
        /// <summary>
        /// Inserts a new activity log entry into the data store.
        /// </summary>
        /// <param name="log">The activity log entry to insert.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task InsertAsync(ActivityLog log, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a paginated list of activity log entries for the specified user,
        /// ordered by most recent first.
        /// </summary>
        /// <param name="userId">The identifier of the user whose logs to retrieve.</param>
        /// <param name="page">The 1-based page number to retrieve.</param>
        /// <param name="pageSize">The number of entries per page.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A read-only list of activity log entries for the requested page.</returns>
        Task<IReadOnlyList<ActivityLog>> GetPagedByUserIdAsync(uint userId, int page, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all activity log entries for the specified user that were recorded
        /// after the given log entry identifier, ordered by most recent first.
        /// </summary>
        /// <param name="userId">The identifier of the user whose logs to retrieve.</param>
        /// <param name="afterId">The exclusive lower bound log entry identifier.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A read-only list of activity log entries recorded after <paramref name="afterId"/>.</returns>
        Task<IReadOnlyList<ActivityLog>> GetLatestAsync(uint userId, ulong afterId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the total number of activity log entries recorded for the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user whose log entries to count.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The total count of activity log entries for the user.</returns>
        Task<ulong> CountByUserIdAsync(uint userId, CancellationToken cancellationToken = default);
    }
}