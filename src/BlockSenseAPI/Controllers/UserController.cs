using BlockSenseAPI.Models.TwoFactorAuth.BackupCode;
using BlockSenseAPI.Models.TwoFactorAuth.Setup;
using BlockSenseAPI.Models.TwoFactorAuth.Verification;
using BlockSenseAPI.Models.User;
using BlockSenseAPI.Services.TwoFactorAuth;
using BlockSenseAPI.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace BlockSenseAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("get")]
        [Authorize]
        public async Task<ActionResult<UserInfo>> GetUserInfoEndpoint()
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
        public async Task<ActionResult<AdditionalUserInfo>> GetAddUserInfoEndpoint()
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
