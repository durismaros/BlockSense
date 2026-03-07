using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace BlockSense.Backend.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public SessionsController(ITokenService tokenService)
        {
            _tokenService = tokenService
                ?? throw new ArgumentNullException(nameof(tokenService));
        }

        [HttpDelete("me/sessions")]
        [Authorize]
        public async Task<IActionResult> Delete([FromBody] SessionRevokeRequest request, CancellationToken cancellationToken)
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
                throw new AuthenticationRequiredException();

            await _tokenService.RevokeSessionAsync(userId, request, cancellationToken);

            return NoContent();
        }

        [HttpDelete("me/sessions/all")]
        [Authorize]
        public async Task<IActionResult> DeleteAll([FromBody] RevokeAllSessionsRequest request, CancellationToken cancellationToken)
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
                throw new AuthenticationRequiredException();

            await _tokenService.RevokeAllSessionsAsync(userId, request, cancellationToken);

            return NoContent();
        }
    }
}
