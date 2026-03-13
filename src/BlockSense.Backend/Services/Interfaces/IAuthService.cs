using BlockSense.Backend.Exceptions.Authentication;
using BlockSense.Backend.Models.Device;
using BlockSense.Contracts.DTOs.Authentication;

namespace BlockSense.Backend.Services.Interfaces
{
    /// <summary>
    /// Defines operations for authenticating users and issuing tokens.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user based on their login credentials and device context.
        /// </summary>
        /// <param name="request">The authentication request containing login and password.</param>
        /// <param name="deviceContext">The device context representing the client device making the request.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>An <see cref="AuthResponse"/> containing access and refresh tokens for the authenticated user.</returns>
        /// <exception cref="InvalidCredentialsException">Thrown if the login or password is invalid.</exception>
        /// <exception cref="ForbiddenException">Thrown if the account is banned.</exception>
        Task<AuthResponse> AuthenticateAsync(AuthRequest request, DeviceContext deviceContext, CancellationToken cancellationToken = default);
    }
}