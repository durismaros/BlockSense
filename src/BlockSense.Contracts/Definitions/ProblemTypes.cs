namespace BlockSense.Contracts.Definitions
{
    public static class StandardizedCodes
    {
        public static class Authentication
        {
            public const string AuthenticationRequired = "AUTH_REQUIRED";

            public const string TwoFactorRequired = "AUTH_2FA_REQUIRED";

            public const string InvalidCredentials = "AUTH_CREDENTIALS_INVALID";

            public const string InvalidClientContext = "AUTH_CLIENT_INVALID";
        }

        public static class TwoFactorAuthentication
        {
            public const string SetupRequired = "AUTH_2FA_SETUP_REQUIRED";
            
            public const string ConfigurationConflict = "AUTH_2FA_CONFIGURATION_CONFLICT";

            public const string BackupCodesCooldown = "AUTH_2FA_BACKUP_COOLDOWN";

            public const string Invalid = "AUTH_2FA_CODE_INVALID";
        }

        public static class Registration
        {
            public const string UsernameTaken = "REGISTRATION_USERNAME_TAKEN";

            public const string EmailTaken = "REGISTRATION_EMAIL_TAKEN";

            public const string InvalidInvitation = "REG_INVITATION_INVALID";
        }

        public static class Generic
        {
            public const string BadRequest = "GENERIC_BAD_REQUEST";

            public const string InternalServerError = "GENERIC_INTERNAL_ERROR";

            public const string NotFound = "GENERIC_NOT_FOUND";

            public const string Forbidden = "GENERIC_FORBIDDEN";
        }

        public static class Client
        {
            public const string Timeout = "CLIENT_TIMEOUT";

            public const string NetworkError = "CLIENT_NETWORK_ERROR";

            public const string RequestCancelled = "CLIENT_REQUEST_CANCELLED";

            public const string UnknownError = "CLIENT_UNKNOWN_ERROR";
        }
    }
}
