using RocksDbSharp;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Utilities.FileManagement
{
    public sealed class LevelDbStorage : IDisposable
    {
        private readonly RocksDb _rocksDb;

        public LevelDbStorage(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            var dbOptions = new DbOptions()
                .SetCreateIfMissing(true)
                .SetCreateMissingColumnFamilies(true)
                .IncreaseParallelism(Environment.ProcessorCount)
                .OptimizeLevelStyleCompaction(64 * 1024 * 1024);

            _rocksDb = RocksDb.Open(dbOptions, path);
        }

        public Task PutAsync<T>(
            string key,
            T value,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (value is null)
                throw new ArgumentNullException(nameof(value));

            cancellationToken.ThrowIfCancellationRequested();

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var valueBytes = JsonSerializer.SerializeToUtf8Bytes(value);

            var writeOptions = new WriteOptions()
                .SetSync(true);

            _rocksDb.Put(
                key: keyBytes,
                value: valueBytes,
                writeOptions: writeOptions);

            return Task.CompletedTask;
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            cancellationToken.ThrowIfCancellationRequested();

            var keyBytes = Encoding.UTF8.GetBytes(key);

            var readOptions = new ReadOptions()
                .SetVerifyChecksums(true)
                .SetFillCache(true);

            var valueBytes = _rocksDb.Get(
                key: keyBytes,
                readOptions: readOptions);

            if (valueBytes is null)
            {
                return Task.FromResult<T?>(default);
            }

            T? value = JsonSerializer.Deserialize<T>(valueBytes);

            return Task.FromResult(value);

        }

        public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            cancellationToken.ThrowIfCancellationRequested();

            var keyBytes = Encoding.UTF8.GetBytes(key);

            var writeOptions = new WriteOptions()
                .SetSync(true);

            _rocksDb.Remove(
                key: keyBytes,
                writeOptions: writeOptions);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _rocksDb.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
