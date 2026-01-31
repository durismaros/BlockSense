using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace BlockSense.Backend.Controllers
{
    [ApiController]
    [Route("api/users/me/2fa")]
    public class TwoFaController : ControllerBase
    {
        private readonly ITwoFactorAuthService _twoFactorAuthService;
        private readonly IUserService _userService;

        public TwoFaController(ITwoFactorAuthService twoFactorAuthService, IUserService userService)
        {
            _twoFactorAuthService = twoFactorAuthService
                ?? throw new ArgumentNullException(nameof(twoFactorAuthService));

            _userService = userService
                ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
                throw new AuthenticationRequiredException();

            var response = await _twoFactorAuthService.SetupInitAsync(userId);

            return Ok(response);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post([FromBody] TwoFactorSetupRequest request, CancellationToken cancellationToken)
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
                throw new AuthenticationRequiredException();

            await _twoFactorAuthService.CompleteSetupAsync(userId, request, cancellationToken);

            var userSummary = await _userService.GetUserSummaryAsync(userId, cancellationToken);

            return Ok(userSummary);
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete([FromBody] TwoFactorVerificationRequest request, CancellationToken cancellationToken)
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
                throw new AuthenticationRequiredException();

            await _twoFactorAuthService.DisableAsync(userId, request, cancellationToken);

            var userSummary = await _userService.GetUserSummaryAsync(userId, cancellationToken);

            return Ok(userSummary);
        }

        [HttpGet("backup")]
        [Authorize]
        public async Task<IActionResult> GetBackup(CancellationToken cancellationToken)
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
                throw new AuthenticationRequiredException();

            var response = await _twoFactorAuthService.GenerateBackupCodesAsync(userId, cancellationToken);

            return Ok(response);
        }
    }
}
