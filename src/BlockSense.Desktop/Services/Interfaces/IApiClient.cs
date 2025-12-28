using BlockSense.Desktop.Models.Api;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface IApiClient
    {
        Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken);
        Task<ApiResponse<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken);
    }
}
