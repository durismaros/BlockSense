namespace BlockSense.Contracts.Definitions
{
    /// <summary>
    /// Defines standardized response codes used across the application for consistent error
    /// and status reporting. Codes are grouped by domain and follow an uppercase snake-case convention.
    /// </summary>
    public static class StandardizedCodes
    {
        /// <summary>
        /// Response codes related to authentication failures and requirements.
        /// </summary>
        public static class Authentication
        {
            /// <summary>
            /// The request requires the user to be authenticated.
            /// </summary>
            public const string AuthenticationRequired = "AUTH_REQUIRED";

            /// <summary>
            /// Two-factor authentication verification is required to proceed.
            /// </summary>
            public const string TwoFactorRequired = "AUTH_2FA_REQUIRED";

            /// <summary>
            /// The provided credentials are invalid.
            /// </summary>
            public const string InvalidCredentials = "AUTH_CREDENTIALS_INVALID";

            /// <summary>
            /// The client context (e.g., device or session) is invalid or unrecognized.
            /// </summary>
            public const string InvalidClientContext = "AUTH_CLIENT_INVALID";
        }

        /// <summary>
        /// Response codes related to two-factor authentication configuration and validation.
        /// </summary>
        public static class TwoFactorAuthentication
        {
            /// <summary>
            /// Two-factor authentication setup must be completed before proceeding.
            /// </summary>
            public const string SetupRequired = "AUTH_2FA_SETUP_REQUIRED";

            /// <summary>
            /// A conflicting two-factor authentication configuration already exists.
            /// </summary>
            public const string ConfigurationConflict = "AUTH_2FA_CONFIGURATION_CONFLICT";

            /// <summary>
            /// Backup code regeneration is temporarily unavailable due to a cooldown period.
            /// </summary>
            public const string BackupCodesCooldown = "AUTH_2FA_BACKUP_COOLDOWN";

            /// <summary>
            /// The provided two-factor authentication code is invalid or expired.
            /// </summary>
            public const string Invalid = "AUTH_2FA_CODE_INVALID";
        }

        /// <summary>
        /// Response codes related to user registration failures.
        /// </summary>
        public static class Registration
        {
            /// <summary>
            /// The requested username is already in use.
            /// </summary>
            public const string UsernameTaken = "REGISTRATION_USERNAME_TAKEN";

            /// <summary>
            /// The provided email address is already registered.
            /// </summary>
            public const string EmailTaken = "REGISTRATION_EMAIL_TAKEN";

            /// <summary>
            /// The provided invitation code is invalid or has already been used.
            /// </summary>
            public const string InvalidInvitation = "REG_INVITATION_INVALID";
        }

        /// <summary>
        /// Generic response codes for common server-side error conditions.
        /// </summary>
        public static class Generic
        {
            /// <summary>
            /// The request is malformed or contains invalid parameters.
            /// </summary>
            public const string BadRequest = "GENERIC_BAD_REQUEST";

            /// <summary>
            /// An unexpected internal server error occurred.
            /// </summary>
            public const string InternalServerError = "INTERNAL_SERVER_ERROR";

            /// <summary>
            /// An external service dependency returned an error or is unavailable.
            /// </summary>
            public const string ExternalServiceError = "EXTERNAL_SERVICE_ERROR";

            /// <summary>
            /// The requested resource could not be found.
            /// </summary>
            public const string NotFound = "GENERIC_NOT_FOUND";

            /// <summary>
            /// The authenticated user does not have permission to access this resource.
            /// </summary>
            public const string Forbidden = "GENERIC_FORBIDDEN";
        }

        /// <summary>
        /// Response codes representing client-side error conditions.
        /// </summary>
        public static class Client
        {
            /// <summary>
            /// The request timed out before a response was received.
            /// </summary>
            public const string Timeout = "CLIENT_TIMEOUT";

            /// <summary>
            /// A network error prevented the request from completing.
            /// </summary>
            public const string NetworkError = "CLIENT_NETWORK_ERROR";

            /// <summary>
            /// The request was cancelled before it could complete.
            /// </summary>
            public const string RequestCancelled = "CLIENT_REQUEST_CANCELLED";

            /// <summary>
            /// An unknown client-side error occurred.
            /// </summary>
            public const string UnknownError = "CLIENT_UNKNOWN_ERROR";
        }
    }
}