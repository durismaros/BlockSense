using LevelDB;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Utilities.FileManagement
{
    public sealed class LevelDbStorage : IDisposable
    {
        private readonly DB _db;
        private readonly object _writeLock = new();

        public LevelDbStorage(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            var option = new Options
            {
                CreateIfMissing = true,
                CompressionLevel = CompressionLevel.NoCompression,
                BlockSize = 4096,
                WriteBufferSize = 4 * 1024 * 1024, // 4MB write buffer
                ParanoidChecks = true
            };

            _db = new DB(option, path);
        }

        public Task PutAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var valueBytes = JsonSerializer.SerializeToUtf8Bytes(value);

            return Task.Run(() =>
            {
                lock (_writeLock)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _db.Put(keyBytes, valueBytes, new WriteOptions
                    {
                        Sync = true
                    });
                }
            }, cancellationToken);
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var keyBytes = Encoding.UTF8.GetBytes(key);

            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bytes = _db.Get(key, new ReadOptions
                {
                    VerifyCheckSums = true,
                    FillCache = true
                });

                if (bytes is null)
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(bytes);

            }, cancellationToken);
        }

        public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            var keyBytes = Encoding.UTF8.GetBytes(key);

            return Task.Run(() =>
            {
                lock (_writeLock)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _db.Delete(keyBytes, new WriteOptions
                    {
                        Sync = true
                    });
                }
            }, cancellationToken);
        }

        public void Dispose()
        {
            _db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
