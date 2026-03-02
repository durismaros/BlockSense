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
        Task<ApiResult> PostAsync<TRequest, TResponse>(string requestUri, TRequest? request, CancellationToken cancellationToken);
        Task<ApiResult> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken);
        Task<ApiResult> PutAsync<TRequest, TResponse>(string requestUri, TRequest? request, CancellationToken cancellationToken);
        Task<ApiResult> DeleteAsync<TRequest, TResponse>(string requestUri, TRequest? request, CancellationToken cancellationToken);
        IApiClient AddBearerToken();
        IApiClient AddDeviceHeaders();
    }
}
