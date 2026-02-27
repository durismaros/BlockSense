namespace BlockSense.Contracts.Definitions
{
    public static class ActivityActions
    {
        public static class Device
        {
            public const string Authenticated = "device.auth.succeeded";

            public const string Revoked = "device.revoked";
        }

        public static class TwoFactorAuthentication
        {
            public const string Enabled = "auth.2fa.enabled";

            public const string Disabled = "auth.2fa.disabled";

            public const string BackupCodesGenerated = "auth.2fa.backup.generated";
        }

        public static class Profile
        {
            public const string UsernameChanged = "profile.username.changed";

            public const string EmailChanged = "profile.email.changed";

            public const string PasswordChanged = "profile.password.changed";

            public const string PictureChanged = "profile.picture.changed";
        }

        public static class User
        {
            public const string Registered = "user.registered";

            public const string RoleUpdated = "user.role.updated";

            public const string Deleted = "user.deleted";

            public const string Restored = "user.restored";

        }

        public static class Invitation
        {
            public const string CodeGenerated = "invitation.code.generated";

            public const string CodeRedeemed = "invitation.code.redeemed";
        }
    }
}
