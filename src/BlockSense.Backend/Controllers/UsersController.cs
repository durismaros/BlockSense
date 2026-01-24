using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Registration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegistrationRequest request, CancellationToken cancellationToken)
        {
            var response = await _userService.RegisterAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(RegisterUserAsync),
                new { response.UserId },
                response);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsersAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetUserByIdAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUsersAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
