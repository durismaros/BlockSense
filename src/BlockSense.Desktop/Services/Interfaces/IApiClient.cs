using BlockSense.Desktop.Models.Api;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface IApiClient
    {
        Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken);
        Task<ApiResult<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken);
    }
}
