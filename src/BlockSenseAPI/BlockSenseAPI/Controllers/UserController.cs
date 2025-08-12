using BlockSenseAPI.Models.Requests;
using BlockSenseAPI.Models.TwoFactorAuth;
using BlockSenseAPI.Models.User;
using BlockSenseAPI.Services.UserServices;
using MaxMind.GeoIP2.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Matching;
using MySqlX.XDevAPI.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlockSenseAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITwoFactorAuthService _twoFactorAuthService;

        public UserController(IUserService userService, ITwoFactorAuthService twoFactorAuthService)
        {
            _userService = userService;
            _twoFactorAuthService = twoFactorAuthService;
        }

        [HttpGet("get")]
        [Authorize]
        public async Task<ActionResult<UserInfoModel>> GetUserInfoEndpoint()
        {
            try
            {
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId))
                    return Unauthorized("User ID not found in token");

                var userInfo = await _userService.FetchUserInfo(userId);

                if (userInfo is null)
                    return NotFound("User not found");

                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-additional")]
        [Authorize]
        public async Task<ActionResult<AdditionalUserInfoModel>> GetAddUserInfoEndpoint()
        {
            try
            {
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId))
                    return Unauthorized("User ID not found in token");

                var addUserInfo = await _userService.FetchAddUserInfo(userId);
                
                if (addUserInfo is null)
                    return NotFound("User not found");

                return Ok(addUserInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("otp-setup")]
        [Authorize]
        public async Task<ActionResult<TwoFactorSetupResponseModel>> OtpSetupEndpoint()
        {
            try
            {
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId))
                    return Unauthorized("User ID not found in token");

                var twoFaResponse = await _twoFactorAuthService.BeginSetup(userId);
                
                if (twoFaResponse is null)
                    return NotFound("User not found");

                return Ok(twoFaResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("otp-enable")]
        [Authorize]
        public async Task<ActionResult<TwoFactorVerificationResponse>> EnableOtpEndpoint([FromBody] TwoFactorSetupRequestModel request)
        {
            try
            {
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId))
                    return Unauthorized("User ID not found in token");

                var twoFaResponse = await _twoFactorAuthService.CompleteSetup(userId, request);

                if (!twoFaResponse)
                    return BadRequest("Error");

                return Ok(new TwoFactorVerificationResponse
                {
                    Message = "enabled",
                    Verification = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //[HttpPost("logout")]
        //[Authorize]
        //public async Task<IActionResult> Logout()
        //{
        //    try
        //    {
        //        var tokenId = User.FindFirst("tokenId")?.Value;
        //        if (string.IsNullOrEmpty(tokenId))
        //        {
        //            return BadRequest("Invalid token");
        //        }

        //        await _userService.Logout(tokenId);
        //        return Ok(new { success = true, message = "Logged out successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}
    }
}
