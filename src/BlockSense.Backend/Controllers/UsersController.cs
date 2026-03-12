using BlockSense.Backend.Controllers.Base;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Registration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    /// <summary>
    /// Provides endpoints for user registration and retrieval of the authenticated user's profile data.
    /// </summary>
    [Route("api/users")]
    public class UsersController : AuthenticatedControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="userService">Service used to register and retrieve user data.</param>
        public UsersController(IUserService userService)
        {
            _userService = userService
                ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The registration request containing the new user's details.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>A 201 Created response with the newly created user's ID and summary.</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post(
            [FromBody] RegistrationRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _userService.RegisterAsync(request, cancellationToken);

            return CreatedAtAction(nameof(Post), new { response.UserId }, response);
        }

        /// <summary>
        /// Returns the profile summary of the authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>The authenticated user's profile summary.</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
        {
            uint userId = GetAuthenticatedUserId();

            var response = await _userService.GetUserSummaryAsync(userId, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Returns the dashboard data for the authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>The authenticated user's dashboard data.</returns>
        [HttpGet("me/dashboard")]
        [Authorize]
        public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
        {
            uint userId = GetAuthenticatedUserId();

            var response = await _userService.GetUserDashboardAsync(userId, cancellationToken);

            return Ok(response);
        }
    }
}