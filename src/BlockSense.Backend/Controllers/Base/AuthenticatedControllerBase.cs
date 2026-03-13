using BlockSense.Backend.Exceptions.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace BlockSense.Backend.Controllers.Base
{
    /// <summary>
    /// Base controller for endpoints that require an authenticated user.
    /// Provides a helper for resolving the current user's ID from the JWT subject claim.
    /// </summary>
    [ApiController]
    public abstract class AuthenticatedControllerBase : ControllerBase
    {
        /// <summary>
        /// Resolves the authenticated user's ID from the JWT <c>sub</c> claim.
        /// </summary>
        /// <returns>The current user's ID as a <see cref="uint"/>.</returns>
        /// <exception cref="AuthenticationRequiredException">
        /// Thrown when the <c>sub</c> claim is absent or cannot be parsed as a <see cref="uint"/>.
        /// </exception>
        protected uint GetAuthenticatedUserId()
        {
            if (!uint.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out uint userId))
            {
                throw new AuthenticationRequiredException();
            }

            return userId;
        }
    }
}