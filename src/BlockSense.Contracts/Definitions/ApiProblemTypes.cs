namespace BlockSense.Contracts.Definitions
{
    public static class StandardizedCodes
    {
        public static class Authentication
        {
            public const string AuthenticationRequired = "auth.required";

            public const string InvalidCredentials = "auth.credentials.invalid";

            public const string InvalidClientContext = "auth.client.invalid";

            public const string TwoFactorRequired = "auth.2fa.required";
        }

        public static class TwoFactorAuthentication
        {
            public const string Enabled = "auth.2fa.enabled";

            public const string Disabled = "auth.2fa.disabled";

            public const string ConfigurationConflict = "auth.2fa.configuration_conflict";

            public const string SetupRequired = "auth.2fa.setup_required";

            public const string Invalid = "auth.2fa.code.invalid";

            public const string Verified = "auth.2fa.code.verified";

            public const string BackupCodesCooldown = "auth.2fa.backup_codes.cooldown";

            public const string BackupCodesRegenerated = "auth.2fa.backup_codes.regenerated";
        }

        public static class Registration
        {
            public const string InvalidInvitation = "registration.invitation.invalid";

            public const string UsernameTaken = "registration.username.taken";

            public const string EmailTaken = "registration.email.taken";

            public const string UserRegistered = "registration.user.registered";

            public const string InvitationUsed = "registration.invitation.used";
        }

        public static class Device
        {
            public const string Authenticated = "device.authenticated";

            public const string Revoked = "device.revoked";
        }

        public static class Profile
        {
            public const string PictureChanged = "profile.picture.changed";

            public const string EmailChanged = "profile.email.changed";

            public const string UsernameChanged = "profile.username.changed";

            public const string PasswordChanged = "profile.password.changed";
        }

        public static class Generic
        {
            public const string BadRequest = "generic.bad_request";

            public const string InternalServerError = "generic.internal_error";

            public const string Forbidden = "generic.forbidden";

            public const string NotFound = "generic.not_found";
        }

        public static class Client
        {
            public const string Timeout = "client.timeout";

            public const string NetworkError = "client.network_error";

            public const string RequestCancelled = "client.request_cancelled";

            public const string UnknownError = "client.unknown_error";
        }
    }
}
