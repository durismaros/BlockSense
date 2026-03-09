using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace BlockSense.Backend.Controllers
{
    [ApiController]
    [Route("api/users/me/activity")]
    public class ActivityLogController : ControllerBase
    {
        private readonly IActivityLogService _activityLogService;

        public ActivityLogController(IActivityLogService activityLogService)
        {
            _activityLogService = activityLogService
                ?? throw new ArgumentNullException(nameof(activityLogService));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPage(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
                throw new AuthenticationRequiredException();

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var result = await _activityLogService.GetPageAsync(userId, page, pageSize, cancellationToken);

            return Ok(result);
        }

        [HttpGet("latest")]
        [Authorize]
        public async Task<IActionResult> GetNewerThan([FromQuery] ulong afterId, CancellationToken cancellationToken = default)
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
                throw new AuthenticationRequiredException();

            var entries = await _activityLogService.GetLatestAsync(userId, afterId, cancellationToken);

            return Ok(entries);
        }
    }
}
