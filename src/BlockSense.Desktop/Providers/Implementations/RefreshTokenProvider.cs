using BlockSense.Contracts.DTOs.Token;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Utilities.FileManagement;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

        public async Task SaveAsync(RefreshTokenDto refreshToken)
        {
            var json = JsonSerializer.Serialize(refreshToken);
            var bytes = Encoding.UTF8.GetBytes(json);           

            if (OperatingSystem.IsWindows())
            {
                bytes = ProtectedData.Protect(
                    bytes,
                    null,
                    DataProtectionScope.CurrentUser);
            }

            await File.WriteAllBytesAsync(_filePath, bytes);
        }

        public async Task<RefreshTokenDto?> GetAsync()
        {
            if (File.Exists(_filePath) == false)
            {
                return null;
            }

            var bytes = await File.ReadAllBytesAsync(_filePath);

            if (OperatingSystem.IsWindows())
            {
                bytes = ProtectedData.Unprotect(
                    bytes,
                    null,
                    DataProtectionScope.CurrentUser);
            }

            var json = Encoding.UTF8.GetString(bytes);

            return JsonSerializer.Deserialize<RefreshTokenDto>(json);
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
