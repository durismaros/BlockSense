using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

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
        public static byte[] ComputeBytes(byte[] key, byte[] data)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (data is null) throw new ArgumentNullException(nameof(data));

            var hmac = InitializeHmac(key);
            return FinalizeHmac(hmac, data);
        }

        /// <summary>
        /// Computes an HMAC-SHA256 hash and returns the result as a hexadecimal string.
        /// </summary>
        /// <param name="key">The secret key used for the HMAC. Cannot be null.</param>
        /// <param name="data">The input data to hash. Cannot be null.</param>
        /// <returns>An uppercase hexadecimal string representing the computed HMAC-SHA256 hash.</returns>
        public static string ComputeHex(byte[] key, byte[] data)
            => Convert.ToHexString(ComputeBytes(key, data));

        /// <summary>
        /// Computes an HMAC-SHA256 hash and returns the result as a Base64-encoded string.
        /// </summary>
        /// <param name="key">The secret key used for the HMAC. Cannot be null.</param>
        /// <param name="data">The input data to hash. Cannot be null.</param>
        /// <returns>A Base64-encoded string representing the computed HMAC-SHA256 hash.</returns>
        public static string ComputeBase64(byte[] key, byte[] data)
            => Convert.ToBase64String(ComputeBytes(key, data));

        /// <summary>
        /// Initializes an HMAC-SHA256 instance with the specified key.
        /// </summary>
        /// <param name="key">The secret key to initialize the HMAC with.</param>
        /// <returns>An initialized <see cref="HMac"/> instance ready for data processing.</returns>
        private static HMac InitializeHmac(byte[] key)
        {
            var hmac = new HMac(new Sha256Digest());
            hmac.Init(new KeyParameter(key));
            return hmac;
        }

        /// <summary>
        /// Processes the input data and finalizes the HMAC computation.
        /// </summary>
        /// <param name="hmac">The initialized HMAC instance.</param>
        /// <param name="data">The data to authenticate.</param>
        /// <returns>A byte array containing the final HMAC output.</returns>
        private static byte[] FinalizeHmac(HMac hmac, byte[] data)
        {
            hmac.BlockUpdate(data, 0, data.Length);

            byte[] output = new byte[hmac.GetMacSize()];
            hmac.DoFinal(output, 0);
            return output;
        }
    }
}