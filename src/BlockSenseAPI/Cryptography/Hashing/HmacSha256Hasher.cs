using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockSenseAPI.Cryptography.Hashing
{
    /// <summary>
    /// Provides methods to compute HMAC-SHA256 hashes in byte, Base64, or hexadecimal format.
    /// </summary>
    public static class HmacSha256Hasher
    {
        /// <summary>
        /// Computes an HMAC-SHA256 hash of the given data using the provided key.
        /// </summary>
        /// <param name="key">The secret key used for the HMAC. Must not be null.</param>
        /// <param name="data">The input data to hash. Must not be null.</param>
        /// <returns>A byte array containing the computed HMAC-SHA256 hash.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="data"/> is null.</exception>
        public static byte[] ComputeByte(byte[] key, byte[] data)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var hmac = new HMac(new Sha256Digest());
            hmac.Init(new KeyParameter(key));
            hmac.BlockUpdate(data, 0, data.Length);

            byte[] output = new byte[hmac.GetMacSize()];
            hmac.DoFinal(output, 0);
            return output;
        }

        /// <summary>Computes an HMAC-SHA256 hash and returns the result as a Base64-encoded string.</summary>
        public static string ComputeBase64(byte[] key, byte[] data) => Convert.ToBase64String(ComputeByte(key, data));

        /// <summary>Computes an HMAC-SHA256 hash and returns the result as a hexadecimal string.</summary>
        public static string ComputeHex(byte[] key, byte[] data) => Convert.ToHexString(ComputeByte(key, data));
    }
}
