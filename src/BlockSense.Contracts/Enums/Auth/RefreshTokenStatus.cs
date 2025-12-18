namespace BlockSense.Contracts.Enums.Auth
{
    /// <summary>
    /// Represents the result of a refresh token request.
    /// </summary>
    public enum RefreshTokenStatus
    {
        /// <summary>
        /// The status of the refresh request is unknown or has not been determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// The refresh token was valid and a new access token was issued successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The provided refresh token was invalid, expired, or not recognized by the backend.
        /// </summary>
        InvalidToken,

        /// <summary>
        /// The account associated with the token is locked, banned, or otherwise restricted.
        /// </summary>
        AccountLocked,

        /// <summary>
        /// The refresh request failed due to an unexpected error.
        /// </summary>
        Error
    }
}
