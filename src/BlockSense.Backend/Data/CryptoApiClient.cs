using BlockSense.Backend.Data.Configurations;
using BlockSense.Backend.Exceptions;
using BlockSense.Backend.Exceptions.Generic;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BlockSense.Backend.Data
{
    public sealed class CryptoApiClient
    {
        private static readonly HashSet<int> _safeStatusCodes = new()
        {
            400, // Bad Request
            404, // Not Found
            405, // Method Not Allowed
            409, // Conflict
            422  // Unprocessable Entity
        };

        private readonly HttpClient _httpClient;

        public CryptoApiClient(HttpClient httpClient, IOptions<CryptoConfig> cryptoConfig)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(cryptoConfig.Value.BaseUrl.TrimEnd('/') + "/");
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", cryptoConfig.Value.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public Task<T> GetAsync<T>(string path)
            => SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, path));

        public Task<T> PostAsync<T>(string path, object body)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = JsonContent.Create(body)
            };

            return SendAsync<T>(request);
        }

        private async Task<T> SendAsync<T>(HttpRequestMessage request)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            using var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (!_safeStatusCodes.Contains((int)response.StatusCode))
                {
                    throw new ExternalServiceException();
                }

                var errorEnvelope = JsonSerializer.Deserialize<ErrorEnvelope>(json, options)
                    ?? throw new InvalidOperationException("Received an empty response body.");

                throw new CustomException(
                    errorEnvelope.Error.Code.ToUpperInvariant(),
                    errorEnvelope.Error.Message,
                    (int)response.StatusCode,
                    errorEnvelope.Error.Details?.Message ?? string.Empty);
            }

            return JsonSerializer.Deserialize<T>(json, options)
                   ?? throw new InvalidOperationException("Received an empty response body.");
        }

        private sealed class ErrorEnvelope
        {
            public required ApiError Error { get; set; }
        }

        private sealed class ApiError
        {
            public required string Code { get; set; }
            public required string Message { get; set; }
            public ErrorDetails? Details { get; set; }
        }

        private sealed class ErrorDetails
        {
            public required string Attribute { get; set; }
            public required string Message { get; set; }
        }
    }
}
