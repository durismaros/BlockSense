namespace BlockSenseAPI.Models.Register
{
    /// <summary>
    /// Represents the result of a user registration attempt.
    /// </summary>
    public class RegisterResponse
    {
        /// <summary>
        /// Indicates whether the registration was successful.
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Provides a human-readable message describing the result of the registration.
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Optional user identifier assigned upon successful registration.
        /// </summary>
        public long? UserId { get; init; }

        /// <summary>
        /// Factory method to create a successful registration response.
        /// </summary>
        /// <param name="message">Optional success message.</param>
        /// <param name="userId">Optional user ID of the newly created account.</param>
        public static RegisterResponse SuccessResponse(string? message = "Registration successful. Welcome.", long? userId = null)
        {
            return new RegisterResponse { Success = true, Message = message, UserId = userId };
        }

        /// <summary>
        /// Factory method to create a failed registration response.
        /// </summary>
        /// <param name="message">Error message explaining why registration failed.</param>
        /// <param name="errorCode">Optional machine-readable error code.</param>
        public static RegisterResponse FailureResponse(string message, string? errorCode = null)
        {
            return new RegisterResponse { Success = false, Message = message };
        }
    }
}
