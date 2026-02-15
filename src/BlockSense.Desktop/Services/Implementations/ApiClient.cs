using BlockSense.Contracts.Definitions;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.ApiHandling;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
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
        private readonly NavigationManager _navigationManager;
        private readonly ApiRequestOptions _requestOptions;

        /// <summary>
        /// Initializes a new instance of <see cref="ApiClient"/>.
        /// </summary>
        /// <param name="logger">The logger for capturing HTTP request events.</param>
        /// <param name="httpClient">The HTTP client used to send requests.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="httpClient"/> or <paramref name="logger"/> is null.</exception>
        public ApiClient(ILogger<ApiClient> logger, HttpClient httpClient, NavigationManager navigationManager)
            : this(logger, httpClient, navigationManager, new ApiRequestOptions())
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        }

        private ApiClient(ILogger<ApiClient> logger, HttpClient httpClient, NavigationManager navigationManager, ApiRequestOptions apiRequestOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
            _requestOptions = apiRequestOptions ?? throw new ArgumentNullException(nameof(apiRequestOptions));
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
        private async Task<ApiResult> SendAsync<TRequest, TResponse>(HttpMethod method, string requestUri, TRequest? request, CancellationToken cancellationToken)
        {
            using var httpRequest = new HttpRequestMessage(method, requestUri);
            _requestOptions.ApplyTo(httpRequest);

            if (request is not null)
            {
                httpRequest.Content = JsonContent.Create(request);
            }

            try
            {
                var response = await _httpClient
                    .SendAsync(httpRequest, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content
                        .ReadFromJsonAsync<TResponse>(cancellationToken) ?? Activator.CreateInstance<TResponse>();

                    return new ApiResult<TResponse>.Success(data);
                }

                var problemDetails = await response.Content
                    .ReadFromJsonAsync<ProblemDetails>() ?? new ProblemDetails
                    {
                        Type = StandardizedCodes.Client.UnknownError,
                        Title = "Unknown Error",
                        Status = (int)response.StatusCode,
                        Detail = "The server returned an unexpected error.",
                        Instance = requestUri
                    };

                if (problemDetails.Type is StandardizedCodes.Authentication.AuthenticationRequired)
                {
                    throw new AuthenticationRequiredException();
                }

                return new ApiResult.Failure(problemDetails);

            }
            catch (AuthenticationRequiredException)
            {
                await _navigationManager.NavigateToAsync<AuthenticationView>();

                var problemDetails = new ProblemDetails
                {
                    Type = StandardizedCodes.Authentication.AuthenticationRequired,
                    Title = "Reauthentication Required",
                    Status = 401,
                    Detail = "For security purposes, please reauthenticate.",
                    Instance = requestUri
                };

                return new ApiResult.Failure(problemDetails);
            }
            catch (Exception)
            {
                var problemDetails = new ProblemDetails
                {
                    Type = StandardizedCodes.Client.NetworkError,
                    Title = "Network Error",
                    Status = 503,
                    Detail = "A network or connectivity issue occurred while sending your request.",
                    Instance = requestUri
                };

                return new ApiResult.Failure(problemDetails);
            }
        }

        /// <inheritdoc/>
        public Task<ApiResult> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken)
            => SendAsync<TRequest, TResponse>(HttpMethod.Post, requestUri, request, cancellationToken);

        /// <inheritdoc/>
        public Task<ApiResult> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken)
            => SendAsync<object, TResponse>(HttpMethod.Get, requestUri, null, cancellationToken);

        /// <inheritdoc/>
        public Task<ApiResult> PutAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken)
            => SendAsync<TRequest, TResponse>(HttpMethod.Put, requestUri, request, cancellationToken);

        /// <inheritdoc/>
        public Task<ApiResult> DeleteAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken)
            => SendAsync<TRequest, TResponse>(HttpMethod.Delete, requestUri, request, cancellationToken);

        /// <inheritdoc/>
        public IApiClient AddBearerToken()
            => new ApiClient(_logger, _httpClient, _navigationManager, _requestOptions with { AddBearerToken = true });

        /// <inheritdoc/>
        public IApiClient AddDeviceHeaders()
            => new ApiClient(_logger, _httpClient, _navigationManager, _requestOptions with { AddDeviceHeaders = true });
    }
}
