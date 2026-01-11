using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using SimpleBase;

namespace BlockSense.Contracts.Cryptography.Hashing
{
    /// <summary>
    /// Provides utility methods to compute HMAC-SHA256 hashes using a secret key.
    /// Supports output as raw bytes, Base64, or hexadecimal strings.
    /// </summary>
    public static class HmacSha256Hasher
    {
        /// <summary>
        /// Computes an HMAC-SHA256 hash of the input data using the provided secret key.
        /// </summary>
        /// <param name="key">The secret key used for the HMAC. Cannot be null.</param>
        /// <param name="data">The input data to hash. Cannot be null.</param>
        /// <returns>A byte array containing the computed HMAC-SHA256 hash.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="data"/> is null.</exception>
        public static byte[] ComputeByte(byte[] key, byte[] data)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (data is null) throw new ArgumentNullException(nameof(data));

            // Initialize HMAC with SHA-256 and the provided key
            var hmac = new HMac(new Sha256Digest());
            hmac.Init(new KeyParameter(key));
            hmac.BlockUpdate(data, 0, data.Length);

            // Compute the HMAC output
            byte[] output = new byte[hmac.GetMacSize()];
            hmac.DoFinal(output, 0);
            return output;
        }

        /// <summary>
        /// Computes an HMAC-SHA256 hash and returns the result as a hexadecimal string.
        /// </summary>
        /// <param name="key">The secret key used for the HMAC. Cannot be null.</param>
        /// <param name="data">The input data to hash. Cannot be null.</param>
        /// <returns>A hexadecimal string representing the computed HMAC-SHA256 hash.</returns>
        public static string ComputeHex(byte[] key, byte[] data)
            => Convert.ToHexString(ComputeByte(key, data));

        /// <summary>
        /// Computes an HMAC-SHA256 hash and returns the result as a Base64-encoded string.
        /// </summary>
        /// <param name="key">The secret key used for the HMAC. Cannot be null.</param>
        /// <param name="data">The input data to hash. Cannot be null.</param>
        /// <returns>A Base64-encoded string representing the computed HMAC-SHA256 hash.</returns>
        public static string ComputeBase64(byte[] key, byte[] data)
            => Convert.ToBase64String(ComputeByte(key, data));
    }
}
