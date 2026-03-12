using BlockSense.Contracts.DTOs.TwoFactorAuth.Setup;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    /// <summary>
    /// Defines methods for managing two-factor authentication (2FA) for the current user,
    /// including setup, enabling, disabling, and backup code generation.
    /// </summary>
    public interface ITwoFactorAuthService
    {
        /// <summary>
        /// Retrieves the two-factor authentication setup initialization data,
        /// including the QR code URI and setup key.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="TwoFactorSetupInit"/> containing the setup data required to configure an authenticator app.</returns>
        Task<TwoFactorSetupInit> GetSetupInitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Enables two-factor authentication for the current user.
        /// Prompts the user to enter a verification code from their authenticator app.
        /// </summary>
        /// <param name="setupKey">The setup key returned during 2FA initialization.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task EnableAsync(string setupKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Disables two-factor authentication for the current user.
        /// Prompts the user to confirm the action with their current authenticator code.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task DisableAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a new set of two-factor authentication backup codes for the current user.
        /// The generated codes are stored in the current user provider and available for download.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task GenerateBackupCodesAsync(CancellationToken cancellationToken = default);
    }
}