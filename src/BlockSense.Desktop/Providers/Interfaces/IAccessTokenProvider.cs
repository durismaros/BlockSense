using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Providers.Interfaces
{
    public interface IAccessTokenProvider
    {
        string AccessToken
        {
            get;
        }

        DateTime ExpiresAt
        {
            get;
        }

        Task<string> GetAsync(CancellationToken cancellationToken = default);
    }
}
