using BlockSense.Contracts.DTOs.Registration;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    /// <summary>
    /// Defines methods for managing user accounts and loading user profile data.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Registers a new user account using the provided registration details.
        /// Navigates to the authentication view on success, or displays an error notification on failure.
        /// </summary>
        /// <param name="request">The registration request containing the user's credentials and profile information.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads the dashboard data for the currently authenticated user
        /// and populates the current user provider.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task LoadCurrentUserAsync(CancellationToken cancellationToken = default);
    }
}