using BlockSense.Contracts.Enums;

namespace BlockSense.Contracts.DTOs.Auth
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
        /// Optional human-readable message providing additional information.
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// Optional identifier of the newly created user.
        /// </summary>
        public uint? UserId { get; init; }
    }
}
