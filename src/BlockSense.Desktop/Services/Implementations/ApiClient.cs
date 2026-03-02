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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class ApiClient : IApiClient
    {
        private readonly ILogger<ApiClient> _logger;
        private readonly HttpClient _httpClient;
        private readonly ApiRequestOptions _requestOptions;

        public ApiClient(ILogger<ApiClient> logger, HttpClient httpClient)
            : this(logger, httpClient, new ApiRequestOptions()) { }

        private ApiClient(ILogger<ApiClient> logger, HttpClient httpClient, ApiRequestOptions apiRequestOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _requestOptions = apiRequestOptions ?? throw new ArgumentNullException(nameof(apiRequestOptions));
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
            HttpMethod method, string requestUri, TRequest? request, CancellationToken cancellationToken)
        {
            using var httpRequest = new HttpRequestMessage(method, requestUri);
            _requestOptions.ApplyTo(httpRequest);

            if (request is not null)
            {
                httpRequest.Content = JsonContent.Create(request);
            }

            try
            {
                _logger.LogDebug("{Method} {Uri}", method.Method, requestUri);

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return await ReadSuccessAsync<TResponse>(response, cancellationToken);
                }

                return await ReadFailureAsync(response, requestUri);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error for {Method} {Uri}", method.Method, requestUri);
                return NetworkError(requestUri);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Request timeout for {Method} {Uri}", method.Method, requestUri);
                return TimeoutError(requestUri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error for {Method} {Uri}", method.Method, requestUri);
                return UnexpectedError(requestUri);
            }
        }

        private static async Task<ApiResult> ReadSuccessAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                var data = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken)
                    ?? throw new NullReferenceException();

                return new ApiResult<TResponse>.Success(data);
            }
            catch
            {
                return new ApiResult<TResponse>.Success(default!);
            }
        }

        private async Task<ApiResult> ReadFailureAsync(HttpResponseMessage response, string requestUri)
        {
            ProblemDetails problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>()
                ?? DefaultProblem(response, requestUri);

            if (problemDetails.Type is StandardizedCodes.Authentication.AuthenticationRequired)
            {
                _logger.LogWarning("Authentication required. Forcing sign-out.");

                var sessionService = App.ServiceProvider.GetRequiredService<ISessionService>();
                await sessionService.SignOutAsync();
            }

            _logger.LogWarning(
                "HTTP {StatusCode} from {Uri} — {Type}",
                (int)response.StatusCode, requestUri, problemDetails.Type);

            return new ApiResult.Failure(problemDetails);
        }

        private static ProblemDetails DefaultProblem(HttpResponseMessage response, string uri) => new()
        {
            Type = StandardizedCodes.Client.UnknownError,
            Title = "Unknown Error",
            Status = (int)response.StatusCode,
            Detail = "The server returned an unexpected response.",
            Instance = uri
        };

        private static ApiResult.Failure NetworkError(string uri) => new(new ProblemDetails
        {
            Type = StandardizedCodes.Client.NetworkError,
            Title = "Network Error",
            Status = 503,
            Detail = "A network or connectivity issue occurred. Please check your connection.",
            Instance = uri
        });

        private static ApiResult.Failure TimeoutError(string uri) => new(new ProblemDetails
        {
            Type = StandardizedCodes.Client.NetworkError,
            Title = "Request Timeout",
            Status = 408,
            Detail = "The request took too long to complete. Please try again.",
            Instance = uri
        });

        private static ApiResult.Failure UnexpectedError(string uri) => new(new ProblemDetails
        {
            Type = StandardizedCodes.Client.UnknownError,
            Title = "Unexpected Error",
            Status = 500,
            Detail = "An unexpected error occurred while processing the request.",
            Instance = uri
        });
    }
}
