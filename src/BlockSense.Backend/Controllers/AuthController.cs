using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Models;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Auth.Login;
using BlockSense.Contracts.DTOs.Auth.Register;
using BlockSense.Contracts.Enums.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public sealed class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            if (!HttpContext.Items.TryGetValue("DeviceContext", out var deviceObj) || deviceObj is not DeviceContext deviceContext)
            {
                throw new BadHttpRequestException("Device context missing in HttpContext.");
            }

            var response = await _userService.LoginAsync(request, deviceContext, cancellationToken);

            return Ok(response);
        }
    }
}
