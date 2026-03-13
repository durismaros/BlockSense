using BlockSense.Backend.Controllers.Base;
using BlockSense.Backend.Services.Interfaces;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockSense.Backend.Controllers
{
    /// <summary>
    /// Provides endpoints for managing two-factor authentication for the authenticated user,
    /// including setup, teardown, and backup code generation.
    /// </summary>
    [Route("api/users/me/2fa")]
    public class TwoFaController : AuthenticatedControllerBase
    {
        private readonly ITwoFactorAuthService _twoFactorAuthService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoFaController"/> class.
        /// </summary>
        /// <param name="twoFactorAuthService">Service used to manage two-factor authentication.</param>
        /// <param name="userService">Service used to access user data.</param>
        public TwoFaController(ITwoFactorAuthService twoFactorAuthService, IUserService userService)
        {
            _twoFactorAuthService = twoFactorAuthService
                ?? throw new ArgumentNullException(nameof(twoFactorAuthService));

            // userService is retained for future use; validated eagerly to fail fast on misconfiguration.
            _ = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        /// Initiates the two-factor authentication setup flow for the authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>Setup data required to configure a 2FA authenticator app.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            uint userId = GetAuthenticatedUserId();

            var response = await _twoFactorAuthService.SetupInitAsync(userId);

            return Ok(response);
        }

        /// <summary>
        /// Completes the two-factor authentication setup by verifying the initial code.
        /// </summary>
        /// <param name="request">The setup request containing the confirmation code from the authenticator app.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(
            [FromBody] TwoFactorSetupRequest request,
            CancellationToken cancellationToken)
        {
            uint userId = GetAuthenticatedUserId();

            await _twoFactorAuthService.CompleteSetupAsync(userId, request, cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Disables two-factor authentication for the authenticated user after verifying their code.
        /// </summary>
        /// <param name="request">The verification request used to confirm the user's identity before disabling 2FA.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>A 204 No Content response on success.</returns>
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> Delete(
            [FromBody] TwoFactorVerificationRequest request,
            CancellationToken cancellationToken)
        {
            uint userId = GetAuthenticatedUserId();

            await _twoFactorAuthService.DisableAsync(userId, request, cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Generates a new set of backup codes for the authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        /// <returns>A list of newly generated backup codes.</returns>
        [HttpGet("backup")]
        [Authorize]
        public async Task<IActionResult> GetBackup(CancellationToken cancellationToken)
        {
            uint userId = GetAuthenticatedUserId();

            var response = await _twoFactorAuthService.GenerateBackupCodesAsync(userId, cancellationToken);

            return Ok(response);
        }
    }
}