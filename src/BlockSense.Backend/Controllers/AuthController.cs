using BlockSense.Backend.Models;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
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

            var response = await _authService.AuthenticateAsync(request, deviceContext, cancellationToken);

            return Ok(response);
        }
    }
}
