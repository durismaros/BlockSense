using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Utilities;
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

        public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, CancellationToken cancellationToken)
        {
            return await SendAsync<TRequest, TResponse>(HttpMethod.Post, requestUri, request, cancellationToken);
        }

        public async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken)
        {
            return await SendAsync<object, TResponse>(HttpMethod.Get, requestUri, null!, cancellationToken);
        }

        private async Task<ApiResponse<TResponse>> SendAsync<TRequest, TResponse>(HttpMethod method, string requestUri, TRequest request, CancellationToken cancellationToken)
        {
            var httpRequest = new HttpRequestMessage(method, requestUri);

            if (request is not null)
            {
                string json = JsonSerializer.Serialize(request);
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return new ApiResponse<TResponse>
                {
                    IsSuccess = false,
                    ProblemDetails = new ApiProblemDetails
                    {
                        Title = "Request Timeout",
                        Status = 408,
                        Detail = "The request to the server timed out.",
                        Instance = requestUri,
                        ResultCode = ResultCodes.Client.Timeout,
                        TraceId = string.Empty
                    }
                };

                //_logger.LogWarning("Request to {RequestUri} timed out.", requestUri);
            }
            catch (Exception)
            {
                return new ApiResponse<TResponse>
                {
                    IsSuccess = false,
                    ProblemDetails = new ApiProblemDetails
                    {
                        Title = "Network Error",
                        Status = 503,
                        Detail = "A network or connectivity error occurred while sending the request.",
                        Instance = requestUri,
                        ResultCode = ResultCodes.Client.NetworkError,
                        TraceId = string.Empty
                    }
                };

                //_logger.LogError(ex, "HTTP request to {RequestUri} failed.", requestUri);
            }


            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);

                return new ApiResponse<TResponse>
                {
                    IsSuccess = true,
                    Data = data
                };
            }

            var problemDetails = await response.Content.ReadFromJsonAsync<ApiProblemDetails>();

            return new ApiResponse<TResponse>
            {
                IsSuccess = false,
                ProblemDetails = problemDetails
            };
        }
    }
}
