namespace BlockSense.Contracts.Definitions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// 
        /// </summary>
        public static class Authentication
        {
            /// <summary>
            /// 
            /// </summary>
            public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";

            /// <summary>
            /// 
            /// </summary>
            public const string AccountBanned = "AUTH_ACCOUNT_BANNED";

            /// <summary>
            /// 
            /// </summary>
            public const string TwoFactorRequired = "AUTH_2FA_REQUIRED";

            /// <summary>
            /// 
            /// </summary>
            public const string TokenExpired = "AUTH_TOKEN_EXPIRED";
        }

        /// <summary>
        /// 
        /// </summary>
        public static class Registration
        {
            /// <summary>
            /// 
            /// </summary>
            public const string InvalidInvitation = "REG_INVALID_INVITATION";

            /// <summary>
            /// 
            /// </summary>
            public const string UsernameTaken = "REG_USERNAME_TAKEN";

            /// <summary>
            /// 
            /// </summary>
            public const string EmailTaken = "REG_EMAIL_TAKEN";
        }

        /// <summary>
        /// 
        /// </summary>
        public static class Generic
        {
            /// <summary>
            /// 
            /// </summary>
            public const string BadRequest = "GEN_BAD_REQUEST";

            /// <summary>
            /// 
            /// </summary>
            public const string InternalServerError = "GEN_INTERNAL_ERROR";
        }
    }
}
