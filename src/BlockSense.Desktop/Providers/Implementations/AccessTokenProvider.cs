using BlockSense.Desktop.Providers.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class AccessTokenProvider : IAccessTokenProvider
    {
        private readonly IRefreshTokenProvider _refreshTokenProvider;

        public string AccessToken
        {
            get;
            private set;
        }

        public DateTime ExpiresAt
        {
            get;
            private set;
        }

        public AccessTokenProvider(IRefreshTokenProvider refreshTokenProvider)
        {
            _refreshTokenProvider = refreshTokenProvider ?? throw new ArgumentNullException(nameof(refreshTokenProvider));
            AccessToken = string.Empty;
            ExpiresAt = DateTime.UtcNow;
        }

        public async Task<string> GetAsync(CancellationToken cancellationToken = default)
        {
            if (AccessToken is not null && ExpiresAt < DateTime.UtcNow)
            {
                return AccessToken;
            }

            throw new NotImplementedException();
        }
    }
}
