using BlockSense.Contracts.DTOs.Token;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Utilities.ApiHandling.Exceptions;
using BlockSense.Desktop.Utilities.FileManagement;
using Humanizer;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class RefreshTokenProvider : IRefreshTokenProvider
    {
        private readonly ILogger<RefreshTokenProvider> _logger;
        private readonly string _filePath;

        public RefreshTokenProvider(ILogger<RefreshTokenProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _filePath = DirectoryStructure.GetAuthFilePath("refresh_token.bin");
        }

        public async Task<string> GetAsync(CancellationToken cancellationToken)
        {
            try
            {
                var bytes = await File.ReadAllBytesAsync(_filePath, cancellationToken);

                if (OperatingSystem.IsWindows())
                {
                    bytes = ProtectedData.Unprotect(
                        bytes, null, DataProtectionScope.CurrentUser);
                }

                var refreshToken = JsonSerializer.Deserialize<RefreshTokenDto>(bytes);

                if (refreshToken is null || string.IsNullOrWhiteSpace(refreshToken.Token))
                {
                    _logger.LogWarning("Refresh token file is corrupted");
                    Clear();
                    throw new AuthenticationRequiredException();
                }

                if (refreshToken.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Refresh token expired at {ExpiresAt:O}", refreshToken.ExpiresAt);
                    Clear();
                    throw new AuthenticationRequiredException();
                }

                _logger.LogDebug("Refresh token retrieved (expires {ExpiresAt:O})", refreshToken.ExpiresAt);
                return refreshToken.Token;
            }
            catch
            {
                Clear();
                throw new AuthenticationRequiredException();
            }
        }

        public async Task SaveAsync(RefreshTokenDto refreshToken, CancellationToken cancellationToken)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(refreshToken);

            if (OperatingSystem.IsWindows())
            {
                bytes = ProtectedData.Protect(
                    bytes, null, DataProtectionScope.CurrentUser);
            }

            await File.WriteAllBytesAsync(_filePath, bytes, cancellationToken);

            _logger.LogInformation("Refresh token saved (expires {ExpiresAt:O})", refreshToken.ExpiresAt);
        }

        public void Clear()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
                _logger.LogInformation("Refresh token file deleted");
            }
        }

        /// <inheritdoc/>
        public bool Exists() => File.Exists(_filePath);
    }
}
