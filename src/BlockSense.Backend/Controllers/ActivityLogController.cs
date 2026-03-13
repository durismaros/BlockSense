using BlockSense.Backend.Controllers.Base;
using BlockSense.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    /// <summary>
    /// Provides endpoints for retrieving the authenticated user's activity log.
    /// </summary>
    [Route("api/users/me/activity")]
    public class ActivityLogController : AuthenticatedControllerBase
    {
        private readonly IActivityLogService _activityLogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityLogController"/> class.
        /// </summary>
        /// <param name="activityLogService">Service used to retrieve activity log entries.</param>
        public ActivityLogController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService
                ?? throw new ArgumentNullException(nameof(activityLogService));
        }

        /// <summary>
        /// Returns a paginated page of activity log entries for the authenticated user.
        /// </summary>
        /// <param name="page">The page number to retrieve. Defaults to 1.</param>
        /// <param name="pageSize">The number of entries per page. Clamped between 1 and 100. Defaults to 20.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>A paginated list of activity log entries.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPage(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            uint userId = GetAuthenticatedUserId();

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var result = await _activityLogService.GetPageAsync(userId, page, pageSize, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Returns all activity log entries newer than the specified entry ID for the authenticated user.
        /// </summary>
        /// <param name="afterId">The ID of the last known entry. Only entries with a higher ID are returned.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>A list of activity log entries newer than <paramref name="afterId"/>.</returns>
        [HttpGet("latest")]
        [Authorize]
        public async Task<IActionResult> GetNewerThan(
            [FromQuery] ulong afterId,
            CancellationToken cancellationToken = default)
        {
            uint userId = GetAuthenticatedUserId();

            var entries = await _activityLogService.GetLatestAsync(userId, afterId, cancellationToken);

            return Ok(entries);
        }
    }
}