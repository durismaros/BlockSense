using BlockSense.Contracts.DTOs.User;

namespace BlockSense.Backend.Services.Interfaces
{
    /// <summary>
    /// Defines operations for retrieving user activity logs.
    /// </summary>
    public interface IActivityLogService
    {
        /// <summary>
        /// Retrieves a paginated page of activity log entries for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="page">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of entries per page.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="ActivityLogPageDto"/> containing the paginated log entries and metadata.</returns>
        Task<ActivityLogPageDto> GetPageAsync(uint userId, int page, int pageSize, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the latest activity log entries for the specified user after a given log entry ID.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="afterId">The log entry ID after which entries are retrieved.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A read-only list of <see cref="ActivityLogDto"/> entries.</returns>
        Task<IReadOnlyList<ActivityLogDto>> GetLatestAsync(uint userId, ulong afterId, CancellationToken cancellationToken = default);
    }
}