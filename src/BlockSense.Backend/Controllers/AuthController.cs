using BlockSense.Backend.Attributes;
using BlockSense.Backend.Controllers.Base;
using BlockSense.Backend.Models.Device;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    /// <summary>
    /// Provides endpoints for user authentication, token refresh, and two-factor verification.
    /// </summary>
    [Route("api/auth")]
    public class AuthController : AuthenticatedControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly ITwoFactorAuthService _twoFactorAuthService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">Service used to authenticate users.</param>
        /// <param name="tokenService">Service used to manage and refresh access tokens.</param>
        /// <param name="twoFactorAuthService">Service used to handle two-factor authentication verification.</param>
        public AuthController(
            IAuthService authService,
            ITokenService tokenService,
            ITwoFactorAuthService twoFactorAuthService)
        {
            _authService = authService
                ?? throw new ArgumentNullException(nameof(authService));

            _tokenService = tokenService
                ?? throw new ArgumentNullException(nameof(tokenService));

            _twoFactorAuthService = twoFactorAuthService
                ?? throw new ArgumentNullException(nameof(twoFactorAuthService));
        }

        /// <summary>
        /// Authenticates a user using their credentials and device context.
        /// </summary>
        /// <param name="request">The authentication request containing the user's credentials.</param>
        /// <param name="deviceContext">Contextual information about the requesting device.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>An authentication response containing access and refresh tokens.</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post(
            [FromBody] AuthRequest request,
            [FromDeviceContext] DeviceContext deviceContext,
            CancellationToken cancellationToken)
        {
            var response = await _authService.AuthenticateAsync(request, deviceContext, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// </summary>
        /// <param name="request">The refresh request containing the refresh token.</param>
        /// <param name="deviceContext">Contextual information about the requesting device.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>A new authentication response with a refreshed access token.</returns>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> PostRefresh(
            [FromBody] AuthRefreshRequest request,
            [FromDeviceContext] DeviceContext deviceContext,
            CancellationToken cancellationToken)
        {
            var response = await _tokenService.RefreshAccessTokenAsync(request, deviceContext, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Verifies a two-factor authentication code for the authenticated user.
        /// </summary>
        /// <param name="request">The verification request containing the 2FA code.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>An empty 200 OK response on successful verification.</returns>
        [HttpPost("2fa")]
        [Authorize]
        public async Task<IActionResult> PostTwoFa(
            [FromBody] TwoFactorVerificationRequest request,
            CancellationToken cancellationToken)
        {
            uint userId = GetAuthenticatedUserId();

            await _twoFactorAuthService.VerifyAsync(userId, request, cancellationToken);

            return Ok();
        }
    }
}