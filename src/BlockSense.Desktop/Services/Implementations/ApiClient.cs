using BlockSense.Contracts.Definitions;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    /// <summary>
    /// Implements <see cref="IApiClient"/> to handle HTTP GET and POST requests, deserialize responses, and wrap results in <see cref="ApiResult{T}"/>.
    /// </summary>
    public sealed class ApiClient : IApiClient
    {
        private readonly ILogger<ApiClient> _logger;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of <see cref="ApiClient"/>.
        /// </summary>
        /// <param name="logger">The logger for capturing HTTP request events.</param>
        /// <param name="httpClient">The HTTP client used to send requests.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="logger"/> is null.</exception>
        public ApiClient(ILogger<ApiClient> logger, HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <inheritdoc/>
        public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken)
        {
            return await SendAsync<TRequest, TResponse>(HttpMethod.Post, requestUri, request, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ApiResult<TResponse>> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken)
        {
            return await SendAsync<object, TResponse>(HttpMethod.Get, requestUri, null!, cancellationToken);
        }

        /// <summary>
        /// Sends an HTTP request with the specified method and payload, handling serialization, deserialization, and error mapping to <see cref="ApiResult{TResponse}"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request payload.</typeparam>
        /// <typeparam name="TResponse">The expected response type.</typeparam>
        /// <param name="method">The HTTP method to use (GET, POST, etc.).</param>
        /// <param name="requestUri">The relative API endpoint URI.</param>
        /// <param name="request">The request payload (can be null for GET requests).</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>An <see cref="ApiResult{TResponse}"/> representing the API response.</returns>
        private async Task<ApiResult<TResponse>> SendAsync<TRequest, TResponse>(HttpMethod method, string requestUri, TRequest request, CancellationToken cancellationToken)
        {
            var httpRequest = new HttpRequestMessage(method, requestUri);

            if (request is not null)
            {
                string json = JsonSerializer.Serialize(request);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            try
            {
                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken)
                        ?? Activator.CreateInstance<TResponse>();

                    return ApiResult<TResponse>.Success(data);
                }

                var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>()
                    ?? new ProblemDetails
                    {
                        Type = ApiProblemTypes.Client.UnknownError,
                        Title = "Unknown Error",
                        Status = (int)response.StatusCode,
                        Detail = "The server returned an unknown error.",
                        Instance = requestUri
                    };

                return ApiResult<TResponse>.Failure(problemDetails);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Request to {RequestUri} timed out.", requestUri);

                // Timeout handling
                var problemDetails = new ProblemDetails
                {
                    Type = ApiProblemTypes.Client.Timeout,
                    Title = "Request Timeout",
                    Status = 408,
                    Detail = "The request to the server timed out.",
                    Instance = requestUri
                };

                return ApiResult<TResponse>.Failure(problemDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP request to {RequestUri} failed.", requestUri);

                // Network or connectivity error handling
                var problemDetails = new ProblemDetails
                {
                    Type = ApiProblemTypes.Client.NetworkError,
                    Title = "Network Error",
                    Status = 503,
                    Detail = "A network or connectivity error occurred while sending the request.",
                    Instance = requestUri
                };

                return ApiResult<TResponse>.Failure(problemDetails);
            }
        }
    }
}
