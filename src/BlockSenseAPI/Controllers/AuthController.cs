using BlockSenseAPI.Models.Login;
using BlockSenseAPI.Models.Register;
using BlockSenseAPI.Models.Requests;
using BlockSenseAPI.Models.Token.DTOs;
using BlockSenseAPI.Models.TwoFactorAuth.Verification;
using BlockSenseAPI.Services.Token;
using BlockSenseAPI.Services.TwoFactorAuth;
using BlockSenseAPI.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<LoginResponse>> LoginEndpoint([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _userService.LoginAsync(request);

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
        public async Task<ActionResult<RegisterResponse>> RegisterEndpoint([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _userService.RegisterAsync(request);

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
        public async Task<ActionResult<TokenRefreshResponse>> TokenRefreshEndpoint([FromBody] TokenRefreshRequest request)
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
    }
}
