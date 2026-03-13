using BlockSense.Desktop.Models.Api;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    /// <summary>
    /// Defines methods for sending HTTP requests to a backend API and handling structured responses.
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Returns a new <see cref="IApiClient"/> instance configured to attach a Bearer token to outgoing requests.
        /// </summary>
        /// <returns>A configured <see cref="IApiClient"/> instance with Bearer token support enabled.</returns>
        IApiClient AddBearerToken();

        /// <summary>
        /// Returns a new <see cref="IApiClient"/> instance configured to attach device-specific headers to outgoing requests.
        /// </summary>
        /// <returns>A configured <see cref="IApiClient"/> instance with device headers enabled.</returns>
        IApiClient AddDeviceHeaders();

        /// <summary>
        /// Sends an HTTP POST request to the specified URI with an optional request body.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request body.</typeparam>
        /// <typeparam name="TResponse">The expected type of the response body.</typeparam>
        /// <param name="requestUri">The relative or absolute URI of the endpoint.</param>
        /// <param name="request">The request body to serialize and send. Can be <c>null</c>.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An <see cref="ApiResult"/> representing the outcome of the request.</returns>
        Task<ApiResult> PostAsync<TRequest, TResponse>(string requestUri, TRequest? request, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an HTTP GET request to the specified URI.
        /// </summary>
        /// <typeparam name="TResponse">The expected type of the response body.</typeparam>
        /// <param name="requestUri">The relative or absolute URI of the endpoint.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An <see cref="ApiResult"/> representing the outcome of the request.</returns>
        Task<ApiResult> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an HTTP PUT request to the specified URI with an optional request body.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request body.</typeparam>
        /// <typeparam name="TResponse">The expected type of the response body.</typeparam>
        /// <param name="requestUri">The relative or absolute URI of the endpoint.</param>
        /// <param name="request">The request body to serialize and send. Can be <c>null</c>.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An <see cref="ApiResult"/> representing the outcome of the request.</returns>
        Task<ApiResult> PutAsync<TRequest, TResponse>(string requestUri, TRequest? request, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an HTTP DELETE request to the specified URI with an optional request body.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request body.</typeparam>
        /// <typeparam name="TResponse">The expected type of the response body.</typeparam>
        /// <param name="requestUri">The relative or absolute URI of the endpoint.</param>
        /// <param name="request">The request body to serialize and send. Can be <c>null</c>.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An <see cref="ApiResult"/> representing the outcome of the request.</returns>
        Task<ApiResult> DeleteAsync<TRequest, TResponse>(string requestUri, TRequest? request, CancellationToken cancellationToken);
    }
}