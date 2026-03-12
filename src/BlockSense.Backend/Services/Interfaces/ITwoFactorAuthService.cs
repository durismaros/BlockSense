using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using BlockSense.Contracts.DTOs.TwoFactorAuth.Verification;

namespace BlockSense.Backend.Services.Interfaces
{
    /// <summary>
    /// Defines operations for managing two-factor authentication (TOTP).
    /// </summary>
    public interface ITwoFactorAuthService
    {
        /// <summary>
        /// Initiates the two-factor authentication setup process for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="TwoFactorSetupInit"/> containing the setup key and QR code data.</returns>
        Task<TwoFactorSetupInit> SetupInitAsync(uint userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Completes the two-factor authentication setup by verifying the provided code and persisting the TOTP credential.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="request">The setup request containing the setup key and verification code.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        Task CompleteSetupAsync(uint userId, TwoFactorSetupRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifies a TOTP code or backup code for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="request">The verification request containing the code to validate.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        Task VerifyAsync(uint userId, TwoFactorVerificationRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a new set of backup codes for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A read-only list of plaintext backup codes.</returns>
        Task<IReadOnlyList<string>> GenerateBackupCodesAsync(uint userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Disables two-factor authentication for the specified user after verifying their identity.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="request">The verification request used to confirm the user's identity before disabling.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        Task DisableAsync(uint userId, TwoFactorVerificationRequest request, CancellationToken cancellationToken = default);
    }
}