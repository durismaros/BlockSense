using BlockSense.Contracts.DTOs.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    /// <summary>
    /// Defines methods for managing the user's authenticated session lifecycle,
    /// including initialization, establishment, token refresh, and sign-out.
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Attempts to restore an existing session on application startup
        /// by validating the stored refresh token. Navigates to the appropriate
        /// view based on the result.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task InitializeSessionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Establishes a new authenticated session using the tokens returned after a successful login.
        /// Persists the refresh token, sets the access token, loads user data, and navigates to the home view.
        /// </summary>
        /// <param name="response">The authentication response containing the access and refresh tokens.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task EstablishSessionAsync(AuthResponse response, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to obtain a new access token using the stored refresh token.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns><c>true</c> if the token was refreshed successfully; otherwise, <c>false</c>.</returns>
        Task<bool> RefreshAccessTokenAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Signs the current user out by clearing all session state and navigating to the welcome view.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task SignOutAsync(CancellationToken cancellationToken = default);
    }
}