using LevelDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Client.DataProtection
{
    public class SecureLevelDbBinaryStore : IDisposable
    {
        private readonly DB _db;
        private readonly Options _dbOptions;
        private readonly ReadOptions _readOptions;
        private readonly WriteOptions _writeOptions;

        public SecureLevelDbBinaryStore(string dbPath)
        {
            _dbOptions = new Options
            {
                CreateIfMissing = true,
                CompressionLevel = CompressionLevel.NoCompression,
                BlockSize = 4096,
                WriteBufferSize = 4 * 1024 * 1024, // 4MB write buffer
                ParanoidChecks = true
            };

            _readOptions = new ReadOptions { VerifyCheckSums = true, FillCache = false };
            _writeOptions = new WriteOptions { Sync = true };

            _db = new DB(_dbOptions, dbPath);
        }

        /// <summary>
        /// Stores raw binary data with a key
        /// </summary>
        public void Put(string key, byte[] data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty");
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            _db.Put(keyBytes, data, _writeOptions);
        }

        /// <summary>
        /// Retrieves raw binary data by key
        /// </summary>
        public byte[] Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty");

            var keyBytes = Encoding.UTF8.GetBytes(key);
            return _db.Get(keyBytes, _readOptions);
        }

        /// <summary>
        /// Deletes data by key (secure version that forces disk sync)
        /// </summary>
        public void Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty");

            var keyBytes = Encoding.UTF8.GetBytes(key);
            _db.Delete(keyBytes, _writeOptions);
        }

        public void Dispose()
        {
            _db?.Dispose();
            _dbOptions?.Dispose();
            _readOptions?.Dispose();
            _writeOptions?.Dispose();
        }
    }
}
