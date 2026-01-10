namespace BlockSense.Contracts.Definitions
{
    /// <summary>
    /// Defines standardized API problem type codes used across the BlockSense system.
    /// Categorized by authentication, registration, generic, and client-level issues.
    /// </summary>
    public static class ApiProblemTypes
    {
        /// <summary>
        /// Problem types related to authentication and login operations.
        /// </summary>
        public static class Authentication
        {
            /// <summary>
            /// Occurs when the provided credentials are invalid.
            /// </summary>
            public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";

            /// <summary>
            /// Occurs when the user account has been banned.
            /// </summary>
            public const string AccountBanned = "AUTH_ACCOUNT_BANNED";

            /// <summary>
            /// Indicates that two-factor authentication is required for login.
            /// </summary>
            public const string TwoFactorRequired = "AUTH_2FA_REQUIRED";

            /// <summary>
            /// Indicates that the authentication token has expired.
            /// </summary>
            public const string TokenExpired = "AUTH_TOKEN_EXPIRED";

            /// <summary>
            /// Indicates that authentication was successful.
            /// </summary>
            public const string AuthenticationSuccess = "AUTH_SUCCESS";
        }

        /// <summary>
        /// Problem types related to user registration processes.
        /// </summary>
        public static class Registration
        {
            /// <summary>
            /// Occurs when an invitation code provided during registration is invalid.
            /// </summary>
            public const string InvalidInvitation = "REG_INVALID_INVITATION";

            /// <summary>
            /// Occurs when the chosen username is already taken.
            /// </summary>
            public const string UsernameTaken = "REG_USERNAME_TAKEN";

            /// <summary>
            /// Occurs when the provided email is already associated with another account.
            /// </summary>
            public const string EmailTaken = "REG_EMAIL_TAKEN";

            /// <summary>
            /// Indicates that registration was successful.
            /// </summary>
            public const string RegistrationSuccess = "REG_SUCCESS";
        }

        /// <summary>
        /// Generic problem types applicable across API endpoints.
        /// </summary>
        public static class Generic
        {
            /// <summary>
            /// Represents a bad request, typically due to invalid input.
            /// </summary>
            public const string BadRequest = "GEN_BAD_REQUEST";

            /// <summary>
            /// Represents an internal server error.
            /// </summary>
            public const string InternalServerError = "GEN_INTERNAL_ERROR";
        }

        /// <summary>
        /// Problem types related to client-side or network issues.
        /// </summary>
        public static class Client
        {
            /// <summary>
            /// Indicates a request timeout on the client side.
            /// </summary>
            public const string Timeout = "CLIENT_TIMEOUT";

            /// <summary>
            /// Indicates a network error occurred while processing the request.
            /// </summary>
            public const string NetworkError = "CLIENT_NETWORK_ERROR";

            /// <summary>
            /// Indicates that the request was cancelled by the client.
            /// </summary>
            public const string RequestCancelled = "CLIENT_REQUEST_CANCELLED";

            /// <summary>
            /// Represents an unknown or unclassified client error.
            /// </summary>
            public const string UnknownError = "CLIENT_UNKNOWN_ERROR";
        }
    }
}
