using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Exceptions.Generic;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BlockSense.Backend.Data
{
    /// <summary>
    /// HTTP client for communicating with the Crypto APIs external service.
    /// Handles request construction, response deserialization, and error mapping.
    /// </summary>
    public sealed class CryptoApiClient
    {
        /// <summary>
        /// HTTP status codes that represent expected API-level errors and should be
        /// mapped to a <see cref="CustomException"/> rather than an <see cref="ExternalServiceException"/>.
        /// </summary>
        private static readonly HashSet<int> _handledErrorStatusCodes = new()
        {
            400, // Bad Request
            404, // Not Found
            405, // Method Not Allowed
            409, // Conflict
            422  // Unprocessable Entity
        };

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of <see cref="CryptoApiClient"/> and configures
        /// the underlying <see cref="HttpClient"/> with the base URL and API key.
        /// </summary>
        /// <param name="httpClient">The HTTP client used to send requests.</param>
        /// <param name="cryptoConfig">The crypto API configuration options.</param>
        public CryptoApiClient(HttpClient httpClient, IOptions<CryptoConfig> cryptoConfig)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(cryptoConfig.Value.BaseUrl.TrimEnd('/') + "/");
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", cryptoConfig.Value.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        /// <summary>
        /// Sends an HTTP GET request to the specified path and deserializes the response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
        /// <param name="path">The relative API path to send the request to.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The deserialized response body as <typeparamref name="T"/>.</returns>
        public Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default)
            => SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, path), cancellationToken);

        /// <summary>
        /// Sends an HTTP POST request with a JSON body to the specified path and deserializes the response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
        /// <param name="path">The relative API path to send the request to.</param>
        /// <param name="body">The request body to serialize as JSON.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The deserialized response body as <typeparamref name="T"/>.</returns>
        public Task<T> PostAsync<T>(string path, object body, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = JsonContent.Create(body)
            };

            return SendAsync<T>(request, cancellationToken);
        }

        /// <summary>
        /// Sends the provided HTTP request and returns the deserialized response body.
        /// Maps API-level errors to typed exceptions based on the response status code.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the successful response body into.</typeparam>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>The deserialized response body as <typeparamref name="T"/>.</returns>
        /// <exception cref="ExternalServiceException">
        /// Thrown when the response status code is not successful and not in <see cref="_handledErrorStatusCodes"/>.
        /// </exception>
        /// <exception cref="CustomException">
        /// Thrown when the response status code is in <see cref="_handledErrorStatusCodes"/>,
        /// containing the error code and message from the API response body.
        /// </exception>
        /// <exception cref="InvalidOperationException">Thrown when the response body is empty or cannot be deserialized.</exception>
        private async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1000, cancellationToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                ThrowForErrorResponse(response, responseBody);
            }

            return DeserializeOrThrow<T>(responseBody);
        }

        /// <summary>
        /// Throws the appropriate exception for a non-successful HTTP response.
        /// </summary>
        /// <param name="response">The non-successful HTTP response.</param>
        /// <param name="responseBody">The raw response body string.</param>
        /// <exception cref="ExternalServiceException">Thrown for unhandled status codes.</exception>
        /// <exception cref="CustomException">Thrown for handled API-level error status codes.</exception>
        private static void ThrowForErrorResponse(HttpResponseMessage response, string responseBody)
        {
            var statusCode = (int)response.StatusCode;

            if (!_handledErrorStatusCodes.Contains(statusCode))
            {
                throw new ExternalServiceException();
            }

            var errorEnvelope = DeserializeOrThrow<ErrorEnvelope>(responseBody);

            throw new CustomException(
                errorEnvelope.Error.Code.ToUpperInvariant(),
                "Crypto API Error",
                statusCode,
                errorEnvelope.Error.Message);
        }

        /// <summary>
        /// Deserializes the provided JSON string into <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized value.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the JSON is empty or deserializes to null.</exception>
        private static T DeserializeOrThrow<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Received an empty response body.");
        }

        /// <summary>
        /// Represents the top-level error envelope returned by the Crypto API on failure.
        /// </summary>
        private sealed class ErrorEnvelope
        {
            /// <summary>
            /// The error details returned by the API.
            /// </summary>
            public required ApiError Error { get; set; }
        }

        /// <summary>
        /// Represents the error details within an API error response.
        /// </summary>
        private sealed class ApiError
        {
            /// <summary>
            /// The machine-readable error code returned by the API.
            /// </summary>
            public required string Code { get; set; }

            /// <summary>
            /// The human-readable error message returned by the API.
            /// </summary>
            public required string Message { get; set; }
        }
    }
}