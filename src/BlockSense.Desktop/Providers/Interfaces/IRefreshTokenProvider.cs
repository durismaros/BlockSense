using BlockSense.Contracts.DTOs.Token;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Providers.Interfaces
{
    public interface IRefreshTokenProvider
    {
        Task<string> GetAsync(CancellationToken cancellationToken);
        Task SaveAsync(RefreshTokenDto refreshTokenm, CancellationToken cancellationToken);
        void Clear();
        bool Exists();
    }
}
