using BlockSense.Backend.Exceptions.Registration;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Contracts.DTOs.User;

namespace BlockSense.Backend.Services.Interfaces
{
    /// <summary>
    /// Defines operations for managing user accounts and retrieving user data.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Registers a new user account using the provided registration details.
        /// </summary>
        /// <param name="request">The registration request containing username, email, password, and invitation code.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="RegistrationResponse"/> containing information about the newly created user.</returns>
        /// <exception cref="InvalidInvitationCodeException">Thrown if the provided invitation code is invalid, revoked, already used, or expired.</exception>
        /// <exception cref="UsernameTakenException">Thrown if the chosen username is already in use.</exception>
        /// <exception cref="EmailTakenException">Thrown if the provided email address is already associated with another account.</exception>
        Task<RegistrationResponse> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a summary of the specified user's account.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="UserSummaryDto"/> containing basic user information.</returns>
        Task<UserSummaryDto> GetUserSummaryAsync(uint userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves aggregated dashboard data for the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>A <see cref="UserDashboardDto"/> containing profile, sessions, recent activity, and invitations.</returns>
        Task<UserDashboardDto> GetUserDashboardAsync(uint userId, CancellationToken cancellationToken = default);
    }
}