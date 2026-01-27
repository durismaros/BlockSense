using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace BlockSense.Backend.Controllers
{
    [ApiController]
    [Route("api/auth/2fa")]
    public class TwoFaController : ControllerBase
    {
        private readonly ITwoFactorAuthService _twoFactorAuthService;

        public TwoFaController(ITwoFactorAuthService twoFactorAuthService)
        {
            _twoFactorAuthService = twoFactorAuthService ?? throw new ArgumentNullException(nameof(twoFactorAuthService));
        }

        [HttpGet("setup")]
        [Authorize]
        public async Task<IActionResult> SetupInit()
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
                throw new AuthenticationRequiredException();

            var response = await _twoFactorAuthService.SetupInitAsync(userId);

            return Ok(response);
        }
    }
}
