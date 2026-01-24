using BlockSense.Contracts.DTOs.Token;
using BlockSense.Desktop.Providers.Implementations;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Providers.Interfaces
{
    public interface IAccessTokenProvider
    {
        public event AccessTokenRefreshRequestedAsync? RefreshRequested;
        Task<string> GetAsync(CancellationToken cancellationToken);
        void Set(AccessTokenDto accessToken);
    }
}
