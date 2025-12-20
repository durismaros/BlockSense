using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Auth.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;

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
            if (!ModelState.IsValid)
            {
                return BadRequest(new RegistrationResponse
                {
                    Status = Contracts.Enums.Auth.RegistrationStatus.Unknown,
                    Message = "The registration request is invalid."
                });
            }

            request = new RegistrationRequest
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Password = request.Password,
                InvitationCode = request.InvitationCode.Trim()
            };

            var response = await _userService.RegisterAsync(request, cancellationToken);

            switch (response.Status)
            {
                case Contracts.Enums.Auth.RegistrationStatus.Success:
                    // Return 201 Created with the new userId in the route
                    return CreatedAtAction(
                        nameof(Register),
                        routeValues: new { userId = response.UserId },
                        value: response);

                case Contracts.Enums.Auth.RegistrationStatus.UsernameTaken:
                case Contracts.Enums.Auth.RegistrationStatus.EmailTaken:
                    // Return 409 Conflict for duplicate username/email
                    return Conflict(response);

                case Contracts.Enums.Auth.RegistrationStatus.InvalidInvitationCode:
                    // Return 400 Bad Request for invalid invitation
                    return BadRequest(response);

                default:
                    // Unexpected status → internal server error
                    return StatusCode(500, response);
            }
        }
    }
}
