using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Exceptions.User;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Registration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace BlockSense.Backend.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService
                ?? throw new ArgumentNullException(nameof(userService));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] RegistrationRequest request, CancellationToken cancellationToken)
        {
            var response = await _userService.RegisterAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof( Post ),
                new { response.UserId },
                response);
        }

        /*
        [HttpGet]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "AdministratorPolicy")]
        public async Task<IActionResult> GetById([FromRoute] int userId, CancellationToken cancellationToken)
        {
            if (userId <= 0)
                throw new UserNotFoundException();

            var response = await _userService.GetUserSummaryAsync((uint)userId, cancellationToken);

            return Ok(response);
        }
        */

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
                throw new AuthenticationRequiredException();

            var response = await _userService.GetUserSummaryAsync(userId, cancellationToken);

            return Ok(response);
        }

        [HttpGet("me/dashboard")]
        [Authorize]
        public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
                throw new AuthenticationRequiredException();

            var response = await _userService.GetUserDashboardAsync(userId, cancellationToken);

            return Ok(response);
        }
    }
}
