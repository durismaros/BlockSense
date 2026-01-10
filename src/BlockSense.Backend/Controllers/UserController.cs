using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Registration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request, CancellationToken cancellationToken)
        {
            var response = await _userService.RegisterAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(Register),
                new { response.UserId },
                response);
        }
    }
}
