using BlockSense.Backend.Attributes;
using BlockSense.Backend.Models.DeviceContext;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AuthenticateAsync([FromBody] AuthRequest request, [FromDeviceContext] DeviceContext deviceContext, CancellationToken cancellationToken)
        {
            var response = await _authService.AuthenticateAsync(request, deviceContext, cancellationToken);

            return Ok(response);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] AuthRefreshRequest request, [FromDeviceContext] DeviceContext deviceContext, CancellationToken cancellationToken)
        {
            var response = await _tokenService.RefreshAccessTokenAsync(request, deviceContext, cancellationToken);

            return Ok(response);
        }
    }
}
