using BlockSense.Contracts.Definitions;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.ApiHandling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    /// <summary>
    /// Implements <see cref="IApiClient"/> to send HTTP requests to the backend API
    /// and return structured <see cref="ApiResult"/> responses.
    /// </summary>
    public sealed class ApiClient : IApiClient
    {
        private readonly ILogger<ApiClient> _logger;
        private readonly HttpClient _httpClient;
        private readonly ApiRequestOptions _requestOptions;

        /// <summary>
        /// Initializes a new instance of <see cref="ApiClient"/> with default request options.
        /// </summary>
        /// <param name="logger">The logger used to record request and error events.</param>
        /// <param name="httpClient">The underlying HTTP client used to send requests.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> or <paramref name="httpClient"/> is null.</exception>
        public ApiClient(ILogger<ApiClient> logger, HttpClient httpClient)
            : this(logger, httpClient, new ApiRequestOptions()) { }

        private ApiClient(ILogger<ApiClient> logger, HttpClient httpClient, ApiRequestOptions requestOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _requestOptions = requestOptions ?? throw new ArgumentNullException(nameof(requestOptions));
        }

        /// <inheritdoc/>
        public IApiClient AddBearerToken()
            => new ApiClient(_logger, _httpClient, _requestOptions with { AddBearerToken = true });

        /// <inheritdoc/>
        public IApiClient AddDeviceHeaders()
            => new ApiClient(_logger, _httpClient, _requestOptions with { AddDeviceHeaders = true });

        /// <inheritdoc/>
        public Task<ApiResult> PostAsync<TRequest, TResponse>(string requestUri, TRequest? request, CancellationToken cancellationToken)
            => SendAsync<TRequest, TResponse>(HttpMethod.Post, requestUri, request, cancellationToken);

        /// <inheritdoc/>
        public Task<ApiResult> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken)
            => SendAsync<object, TResponse>(HttpMethod.Get, requestUri, null, cancellationToken);

        /// <inheritdoc/>
        public Task<ApiResult> PutAsync<TRequest, TResponse>(string requestUri, TRequest? request, CancellationToken cancellationToken)
            => SendAsync<TRequest, TResponse>(HttpMethod.Put, requestUri, request, cancellationToken);

        /// <inheritdoc/>
        public Task<ApiResult> DeleteAsync<TRequest, TResponse>(string requestUri, TRequest? request, CancellationToken cancellationToken)
            => SendAsync<TRequest, TResponse>(HttpMethod.Delete, requestUri, request, cancellationToken);

        private async Task<ApiResult> SendAsync<TRequest, TResponse>(
            HttpMethod method,
            string requestUri,
            TRequest? request,
            CancellationToken cancellationToken)
        {
            using var httpRequest = BuildHttpRequest(method, requestUri, request);

            try
            {
                _logger.LogDebug("Sending {Method} {Uri}", method.Method, requestUri);

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                return response.IsSuccessStatusCode
                    ? await ReadSuccessAsync<TResponse>(response, cancellationToken)
                    : await ReadFailureAsync(response, requestUri);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during {Method} {Uri}", method.Method, requestUri);
                return BuildNetworkError(requestUri);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Request timed out during {Method} {Uri}", method.Method, requestUri);
                return BuildTimeoutError(requestUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during {Method} {Uri}", method.Method, requestUri);
                return BuildUnexpectedError(requestUri);
            }
        }

        private HttpRequestMessage BuildHttpRequest<TRequest>(HttpMethod method, string requestUri, TRequest? request)
        {
            var httpRequest = new HttpRequestMessage(method, requestUri);
            _requestOptions.ApplyTo(httpRequest);

            if (request is not null)
            {
                httpRequest.Content = JsonContent.Create(request);
            }

            return httpRequest;
        }

        private static async Task<ApiResult> ReadSuccessAsync<TResponse>(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            try
            {
                var data = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken)
                    ?? throw new NullReferenceException("Deserialized response was null.");

                return new ApiResult<TResponse>.Success(data);
            }
            catch
            {
                return new ApiResult<TResponse>.Success(default!);
            }
        }

        private async Task<ApiResult> ReadFailureAsync(HttpResponseMessage response, string requestUri)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>()
                ?? throw new NullReferenceException("Failed to deserialize ProblemDetails from error response.");

            if (problemDetails.Type is StandardizedCodes.Authentication.AuthenticationRequired)
            {
                _logger.LogWarning("Authentication required — forcing sign-out");
                await ForceSignOutAsync();
            }

            _logger.LogWarning(
                "HTTP {StatusCode} from {Uri} — {Type}: {Title}",
                (int)response.StatusCode, requestUri, problemDetails.Type, problemDetails.Title);

            return new ApiResult.Failure(problemDetails);
        }

        private static async Task ForceSignOutAsync()
        {
            var sessionService = App.ServiceProvider.GetRequiredService<ISessionService>();
            await sessionService.SignOutAsync();
        }

        private static ApiResult.Failure BuildNetworkError(string requestUri) => new(new ProblemDetails
        {
            Type = StandardizedCodes.Client.NetworkError,
            Title = "Network Error",
            Status = 503,
            Detail = "A network or connectivity issue occurred. Please check your connection.",
            Instance = requestUri
        });

        private static ApiResult.Failure BuildTimeoutError(string requestUri) => new(new ProblemDetails
        {
            Type = StandardizedCodes.Client.NetworkError,
            Title = "Request Timeout",
            Status = 408,
            Detail = "The request took too long to complete. Please try again.",
            Instance = requestUri
        });

        private static ApiResult.Failure BuildUnexpectedError(string requestUri) => new(new ProblemDetails
        {
            Type = StandardizedCodes.Client.UnknownError,
            Title = "Unexpected Error",
            Status = 500,
            Detail = "An unexpected error occurred while processing the request.",
            Instance = requestUri
        });
    }
}