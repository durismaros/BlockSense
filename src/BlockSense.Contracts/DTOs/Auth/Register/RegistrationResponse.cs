using BlockSense.Contracts.Enums.User;
using System.Text.Json.Serialization;

namespace BlockSense.Contracts.DTOs.Auth.Register
{
    /// <summary>
    /// Represents the response returned by the backend after a user registration attempt.
    /// </summary>
    public sealed record RegistrationResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public required uint UserId { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public required string Email { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public required string Username { get; init; }

        /// <summary>
        /// 
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required UserType UserType { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public required DateTime CreatedAt { get; init; }
    }
}
