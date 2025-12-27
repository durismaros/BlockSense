using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Models;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Auth;
using BlockSense.Contracts.DTOs.Auth.Login;
using BlockSense.Contracts.DTOs.Auth.Register;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Contracts.Enums.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Authenticate([FromBody] AuthRequest request, CancellationToken cancellationToken)
        {
            if (!HttpContext.Items.TryGetValue("DeviceContext", out var deviceObj) || deviceObj is not DeviceContext deviceContext)
            {
                throw new BadHttpRequestException("Device context missing in HttpContext.");
            }

            var response = await _authService.AuthAsync(request, deviceContext, cancellationToken);

            return Ok(response);
        }
    }
}
