namespace BlockSense.Contracts.Definitions
{
    /// <summary>
    /// Defines standardized activity action identifiers used for audit logging and event tracking.
    /// Actions are grouped by domain and follow a dot-delimited naming convention.
    /// </summary>
    public static class ActivityActions
    {
        /// <summary>
        /// Activity actions related to device management and authentication.
        /// </summary>
        public static class Device
        {
            /// <summary>
            /// A device successfully completed authentication.
            /// </summary>
            public const string Authenticated = "device.auth.succeeded";

            /// <summary>
            /// A device was revoked and is no longer authorized.
            /// </summary>
            public const string Revoked = "device.revoked";
        }

        /// <summary>
        /// Activity actions related to two-factor authentication configuration.
        /// </summary>
        public static class TwoFactorAuthentication
        {
            /// <summary>
            /// Two-factor authentication was enabled for an account.
            /// </summary>
            public const string Enabled = "auth.2fa.enabled";

            /// <summary>
            /// Two-factor authentication was disabled for an account.
            /// </summary>
            public const string Disabled = "auth.2fa.disabled";

            /// <summary>
            /// Two-factor authentication backup codes were generated.
            /// </summary>
            public const string BackupCodesGenerated = "auth.2fa.backup.generated";
        }

        /// <summary>
        /// Activity actions related to user profile changes.
        /// </summary>
        public static class Profile
        {
            /// <summary>
            /// The account username was changed.
            /// </summary>
            public const string UsernameChanged = "profile.username.changed";

            /// <summary>
            /// The account email address was changed.
            /// </summary>
            public const string EmailChanged = "profile.email.changed";

            /// <summary>
            /// The account password was changed.
            /// </summary>
            public const string PasswordChanged = "profile.password.changed";

            /// <summary>
            /// The account profile picture was changed.
            /// </summary>
            public const string PictureChanged = "profile.picture.changed";
        }

        /// <summary>
        /// Activity actions related to user account lifecycle management.
        /// </summary>
        public static class User
        {
            /// <summary>
            /// A new user account was registered.
            /// </summary>
            public const string Registered = "user.registered";

            /// <summary>
            /// A user's role was updated.
            /// </summary>
            public const string RoleUpdated = "user.role.updated";

            /// <summary>
            /// A user account was deleted.
            /// </summary>
            public const string Deleted = "user.deleted";

            /// <summary>
            /// A previously deleted user account was restored.
            /// </summary>
            public const string Restored = "user.restored";
        }

        /// <summary>
        /// Activity actions related to invitation code management.
        /// </summary>
        public static class Invitation
        {
            /// <summary>
            /// A new invitation code was generated.
            /// </summary>
            public const string CodeGenerated = "invitation.code.generated";

            /// <summary>
            /// An invitation code was redeemed by a new user.
            /// </summary>
            public const string CodeRedeemed = "invitation.code.redeemed";
        }
    }
}