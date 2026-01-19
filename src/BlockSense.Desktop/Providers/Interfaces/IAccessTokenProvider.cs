using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Providers.Interfaces
{
    public interface IAccessTokenProvider
    {
        Task<string> GetAsync(CancellationToken cancellationToken);
    }
}
