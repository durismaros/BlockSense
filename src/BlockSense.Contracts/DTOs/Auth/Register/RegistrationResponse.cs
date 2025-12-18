using BlockSense.Contracts.Enums.Auth;

namespace BlockSense.Contracts.DTOs.Auth.Register
{
    /// <summary>
    /// Represents the response returned by the backend after a user registration attempt.
    /// </summary>
    public sealed record RegistrationResponse
    {
        /// <summary>
        /// The status of the registration attempt.
        /// </summary>
        public RegistrationStatus Status { get; init; } = RegistrationStatus.Unknown;

        /// <summary>
        /// Optional human-readable message providing additional context.
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Optional identifier of the newly created user.
        /// </summary>
        public uint? UserId { get; init; }
    }
}
