using BlockSense.Contracts.DTOs.Token;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Utilities.ApiHandling;
using BlockSense.Desktop.Utilities.FileManagement;
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
        private readonly string _filePath;

        public RefreshTokenProvider()
        {
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
                        bytes,
                        null,
                        DataProtectionScope.CurrentUser);
                }

                var refreshToken = JsonSerializer.Deserialize<RefreshTokenDto>(bytes);

                if (refreshToken is null ||
                    string.IsNullOrWhiteSpace(refreshToken.Token) ||
                    refreshToken.ExpiresAt < DateTime.UtcNow)
                {
                    throw new AuthenticationRequiredException();
                }

                return refreshToken.Token;
            }
            catch
            {
                await ClearAsync();
                throw new AuthenticationRequiredException();
            }
        }

        public async Task SaveAsync(RefreshTokenDto refreshToken)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(refreshToken);

            if (OperatingSystem.IsWindows())
            {
                bytes = ProtectedData.Protect(
                    bytes,
                    null,
                    DataProtectionScope.CurrentUser);
            }

            await File.WriteAllBytesAsync(_filePath, bytes);
        }

        public Task ClearAsync()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }

            return Task.CompletedTask;
        }
    }
}
