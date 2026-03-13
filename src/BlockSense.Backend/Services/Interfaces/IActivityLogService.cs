using BlockSense.Backend.Models.ActivityLog;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Contracts.Enums;

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

        /// <summary>
        /// Creates a new activity log entry.
        /// </summary>
        /// <param name="type">The category of the actor that triggered the event.</param>
        /// <param name="userId">The unique identifier of the user associated with the event.</param>
        /// <param name="action">The stable dot-namespaced event code describing the action that occurred.</param>
        /// <param name="context">Optional structured metadata describing the context of the action.</param>
        /// <param name="cancellationToken">Optional token used to cancel the operation.</param>
        Task CreateAsync(ActivityType type, uint userId, string action, ActivityLogContext? context = null, CancellationToken cancellationToken = default);
    }
}