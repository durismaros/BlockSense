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
    public sealed class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken)
        {
            return await SendAsync<TRequest, TResponse>(HttpMethod.Post, requestUri, request, cancellationToken);
        }

        public async Task<ApiResult<TResponse>> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken)
        {
            return await SendAsync<object, TResponse>(HttpMethod.Get, requestUri, null!, cancellationToken);
        }

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
                    var data = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);

                    return ApiResult<TResponse>.Success(data);
                }

                var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

                return ApiResult<TResponse>.Failure(problemDetails);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                var problemDetails = new ProblemDetails
                {
                    Type = ApiProblemTypes.Client.Timeout,
                    Title = "Request Timeout",
                    Status = 408,
                    Detail = "The request to the server timed out.",
                    Instance = requestUri
                };

                return ApiResult<TResponse>.Failure(problemDetails);

                //_logger.LogWarning("Request to {RequestUri} timed out.", requestUri);
            }
            catch (Exception)
            {
                var problemDetails = new ProblemDetails
                {
                    Type = ApiProblemTypes.Client.NetworkError,
                    Title = "Network Error",
                    Status = 503,
                    Detail = "A network or connectivity error occurred while sending the request.",
                    Instance = requestUri
                };

                return ApiResult<TResponse>.Failure(problemDetails);

                //_logger.LogError(ex, "HTTP request to {RequestUri} failed.", requestUri);
            }
        }
    }
}
