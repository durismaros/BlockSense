using BlockSense.Models.Responses;
using BlockSenseAPI.Models.Requests;
using BlockSenseAPI.Models.Responses;
using BlockSenseAPI.Models.TwoFactorAuth;
using BlockSenseAPI.Services.TokenServices;
using BlockSenseAPI.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace BlockSenseAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ITwoFactorAuthService _twoFactorAuthService;

        public AuthController(IUserService userService, IRefreshTokenService refreshTokenService, ITwoFactorAuthService twoFactorAuthService)
        {
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _twoFactorAuthService = twoFactorAuthService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseModel>> LoginEndpoint([FromBody] LoginRequestModel request)
        {
            try
            {
                var result = await _userService.Login(request);

                if (result is null)
                    return BadRequest();

                if (result.Success)
                    return Ok(result);

                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterResponseModel>> RegisterEndpoint([FromBody] RegisterRequestModel request)
        {
            try
            {
                var result = await _userService.Register(request);

                if (result is null)
                    return BadRequest();

                if (result.Success)
                    return Ok(result);

                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("token-refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenRefreshResponseModel>> TokenRefreshEndpoint([FromBody] TokenRefreshRequestModel request)
        {
            try
            {
                var result = await _refreshTokenService.RefreshAccessToken(request);

                if (result is null)
                    return BadRequest();

                if (result.Success)
                    return Ok(result);

                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("otp-verify")]
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
    }
}
