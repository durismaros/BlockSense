using BlockSense.Backend.Attributes;
using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Models.DeviceContext;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace BlockSense.Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly ITwoFactorAuthService _twoFactorAuthService;

        public AuthController(IAuthService authService, ITokenService tokenService, ITwoFactorAuthService twoFactorAuthService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _twoFactorAuthService = twoFactorAuthService ?? throw new ArgumentNullException(nameof(twoFactorAuthService));
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

        [HttpPost("2fa")]
        [Authorize]
        public async Task<IActionResult> VerifyTwoFaAsync([FromBody] TwoFactorVerificationRequest request, CancellationToken cancellationToken)
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
                throw new AuthenticationRequiredException();

            await _twoFactorAuthService.VerifyAsync(userId, request, cancellationToken);

            return Ok();
        }
    }
}
