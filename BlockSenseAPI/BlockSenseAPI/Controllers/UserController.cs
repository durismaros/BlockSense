using BlockSenseAPI.Models.Requests;
using BlockSenseAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("{uid}")]
        public async Task<IActionResult> LoadUserInfo(int uid)
        {
            try
            {
                var userInfo = await _userService.LoadUserInfo(uid);
                return Ok(userInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _userService.Login(request.Login, request.Password, request.Identifiers);
                if (result.correctLogin)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.loginMessage,
                        tokendata = result.tokenData,
                    });
                }
                return BadRequest(new { success = false, message = result.loginMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _userService.Register(
                    request.Username,
                    request.Email,
                    request.Password,
                    request.InvitationCode
                );

                if (result.correctRegister)
                {
                    return Ok(new { success = true, message = result.registerMessage });
                }
                return BadRequest(new { success = false, message = result.registerMessage });
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
