using BlockSenseAPI.Models.TwoFactorAuth.BackupCode;
using BlockSenseAPI.Models.TwoFactorAuth.Setup;
using BlockSenseAPI.Models.TwoFactorAuth.Verification;
using BlockSenseAPI.Services.TwoFactorAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace BlockSenseAPI.Controllers
{
    [ApiController]
    [Route("api/2fa")]
    public class TwoFaController : ControllerBase
    {
        private readonly ITwoFactorAuthService _twoFactorAuthService;

        public TwoFaController(ITwoFactorAuthService twoFactorAuthService)
        {
            _twoFactorAuthService = twoFactorAuthService;
        }

        [HttpGet("setup")]
        [Authorize]
        public async Task<ActionResult<TwoFactorBackupResponse>> TwoFaSetupEndpoint()
        {
            try
            {
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId))
                    return Unauthorized("User ID not found in token");

                var twoFaSetup = await _twoFactorAuthService.BeginSetup(userId);

                if (twoFaSetup is null)
                    return NotFound("User not found");

                return Ok(twoFaSetup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("enable")]
        [Authorize]
        public async Task<ActionResult<TwoFactorVerificationResponse>> EnableTwoFaEndpoint([FromBody] TwoFactorSetupRequest request)
        {
            try
            {
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId))
                    return Unauthorized("User ID not found in token");

                var twoFaResponse = await _twoFactorAuthService.CompleteSetup(userId, request);

                if (twoFaResponse is null)
                    return BadRequest("Error occurred during enabling 2fa");

                return Ok(twoFaResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("verify")]
        [Authorize]
        public async Task<ActionResult<TwoFactorVerificationResponse>> VerifyOtpEndpoint([FromBody] TwoFactorVerificationRequest request)
        {
            try
            {
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId))
                    return Unauthorized("User ID not found in token");


                if (request is null || request.Code is null || request.Code.Length != 6)
                    return BadRequest(new TwoFactorVerificationResponse
                    {
                        Verification = false,
                        Message = "Otp code is not valid"
                    });

                var twoFaResponse = await _twoFactorAuthService.VerifyOtp(userId, request.Code);

                return Ok(twoFaResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("disable")]
        [Authorize]
        public async Task<ActionResult<TwoFactorVerificationResponse>> DisableTwoFaEndpoint([FromBody] TwoFactorVerificationRequest request)
        {
            try
            {
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId))
                    return Unauthorized("User ID not found in token");

                var twoFaResponse = await _twoFactorAuthService.DisableTwoFa(userId, request.Code);

                if (twoFaResponse is null)
                    return BadRequest("Error occurred during disabling 2fa");

                return Ok(twoFaResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("backup-generation")]
        [Authorize]
        public async Task<ActionResult<TwoFactorBackupResponse>> GenerateBackupCodesEndpoint()
        {
            try
            {
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId))
                    return Unauthorized("User ID not found in token");

                var twoFaResponse = await _twoFactorAuthService.GenerateBackupCodes(userId);

                if (twoFaResponse is null)
                    return BadRequest("Error occurred during backup codes generation");

                return Ok(twoFaResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
