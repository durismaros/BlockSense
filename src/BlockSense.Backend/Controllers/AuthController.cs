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
            var deviceContext = new DeviceContext
            {
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                DeviceIdentifier = Request.Headers["X-Device-Id"]!,
                HardwareFingerprint = Request.Headers["X-Hardware-Fingerprint"]!,
                NetworkFingerprint = Request.Headers["X-Network-Fingerprint"]!,
                DeviceOs = Request.Headers["X-Device-OS"]!
            };

            var response = await _userService.LoginAsync(request, deviceContext, cancellationToken);

            switch (response.Status)
            {
                case LoginStatus.Success:
                    return Ok(response);

                case LoginStatus.InvalidPassword:
                case LoginStatus.TwoFactorRequired:
                    return Unauthorized(response);

                case LoginStatus.AccountLocked:
                    return Forbid();

                default:
                    return StatusCode(500, response);
            }
        }
    }
}
