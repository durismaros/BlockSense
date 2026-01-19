using BlockSense.Backend.Attributes;
using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Models.DeviceContext;
using BlockSense.Backend.Repositories.Interfaces;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BlockSense.Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly ITwoFactorAuthService _twoFactorAuthService;

        public AuthController(IUserRepository userRepository, IAuthService authService, ITokenService tokenService, ITwoFactorAuthService twoFactorAuthService)
        {
            _userRepository = userRepository;
            _authService = authService;
            _tokenService = tokenService;
            _twoFactorAuthService = twoFactorAuthService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Authenticate([FromBody] AuthRequest request, [FromDeviceContext] DeviceContext deviceContext, CancellationToken cancellationToken)
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

        [HttpGet("2fa/setup")]
        [Authorize]
        public async Task<IActionResult> SetupInit()
        {
            if (!uint.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out uint userId))
                throw new AuthenticationRequiredException();
            
            var response = await _twoFactorAuthService.SetupInitAsync(userId);

            return Ok(response);
        }
    }
}
