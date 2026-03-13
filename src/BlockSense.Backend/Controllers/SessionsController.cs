using BlockSense.Backend.Controllers.Base;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    /// <summary>
    /// Provides endpoints for managing the authenticated user's active sessions.
    /// </summary>
    [Route("api/users")]
    public class SessionsController : AuthenticatedControllerBase
    {
        private readonly ITokenService _tokenService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionsController"/> class.
        /// </summary>
        /// <param name="tokenService">Service used to revoke user sessions.</param>
        public SessionsController(ITokenService tokenService)
        {
            _tokenService = tokenService
                ?? throw new ArgumentNullException(nameof(tokenService));
        }

        /// <summary>
        /// Revokes a specific session for the authenticated user.
        /// </summary>
        /// <param name="request">The request identifying the session to revoke.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpDelete("me/sessions")]
        [Authorize]
        public async Task<IActionResult> Delete(
            [FromBody] SessionRevokeRequest request,
            CancellationToken cancellationToken)
        {
            uint userId = GetAuthenticatedUserId();

            await _tokenService.RevokeSessionAsync(userId, request, cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Revokes all active sessions for the authenticated user.
        /// </summary>
        /// <param name="request">The request containing any required confirmation data.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpDelete("me/sessions/all")]
        [Authorize]
        public async Task<IActionResult> DeleteAll(
            [FromBody] RevokeAllSessionsRequest request,
            CancellationToken cancellationToken)
        {
            uint userId = GetAuthenticatedUserId();

            await _tokenService.RevokeAllSessionsAsync(userId, request, cancellationToken);

            return NoContent();
        }
    }
}