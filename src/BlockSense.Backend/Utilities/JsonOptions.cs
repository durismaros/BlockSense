using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlockSense.Backend.Utilities
{
    /// <summary>
    /// Provides shared JSON serializer configuration used across the backend.
    /// </summary>
    public static class JsonOptions
    {
        /// <summary>
        /// Default JSON serializer configuration.
        /// </summary>
        public static readonly JsonSerializerOptions Default = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }
}
