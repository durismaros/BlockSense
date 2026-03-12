using BlockSense.Contracts.Enums;
using System.Text.Json.Serialization;

namespace BlockSense.Contracts.DTOs.Registration
{
    /// <summary>
    /// Represents the response returned after a successful user registration.
    /// </summary>
    public sealed record RegistrationResponse
    {
        /// <summary>
        /// The unique identifier of the newly registered user.
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
        /// The role assigned to the registered user. Serialized as a string in JSON responses.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required UserRole UserRole
        {
            get;
            init;
        }

        /// <summary>
        /// The UTC timestamp when the user account was created.
        /// </summary>
        public required DateTime CreatedAt
        {
            get;
            init;
        }
    }
}