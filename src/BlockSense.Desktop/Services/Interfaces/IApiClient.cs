using BlockSense.Desktop.Models.Api;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    /// <summary>
    /// Defines methods for sending HTTP requests to a backend API and handling structured responses.
    /// </summary>
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Sends an HTTP POST request to the specified endpoint with the provided request payload, returning a strongly-typed <see cref="ApiResult{TResponse}"/>.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request payload.</typeparam>
        /// <typeparam name="TResponse">The expected response type.</typeparam>
        /// <param name="endpoint">The API endpoint relative to the base URL.</param>
        /// <param name="request">The request payload to send.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>An <see cref="ApiResult{TResponse}"/> representing success or failure.</returns>
        Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken);
        /// <summary>
        /// Sends an HTTP GET request to the specified endpoint, returning a strongly-typed <see cref="ApiResult{TResponse}"/>.
        /// </summary>
        /// <typeparam name="TResponse">The expected response type.</typeparam>
        /// <param name="endpoint">The API endpoint relative to the base URL.</param>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        /// <returns>An <see cref="ApiResult{TResponse}"/> representing success or failure.</returns>
        Task<ApiResult<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken);
    }
}
