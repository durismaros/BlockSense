using BlockSense.Contracts.Enums;
using System.Text.Json.Serialization;

namespace BlockSense.Contracts.DTOs.Registration
{
    /// <summary>
    /// Represents the response returned by the backend after a user registration attempt.
    /// </summary>
    public sealed record RegistrationResponse
    {
        /// <summary>
        /// The unique identifier of the registered user.
        /// </summary>
        public required uint UserId
        {
            get;
            init;
        }

        /// <summary>
        /// The email address of the registered user.
        /// </summary>
        public required string Email
        {
            get;
            init;
        }

        /// <summary>
        /// The username of the registered user.
        /// </summary>
        public required string Username
        {
            get;
            init;
        }

        /// <summary>
        /// The type of the registered user.
        /// </summary>
        /// <remarks>
        /// Serialized as a string using JSON string enum converter.
        /// </remarks>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required UserRole UserRole
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC timestamp when the user was created.
        /// </summary>
        public required DateTime CreatedAt
        {
            get;
            init;
        }
    }
}
