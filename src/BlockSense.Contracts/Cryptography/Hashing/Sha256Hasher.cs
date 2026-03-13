using Org.BouncyCastle.Crypto.Digests;

namespace BlockSense.Contracts.Cryptography.Hashing
{
    /// <summary>
    /// Provides utility methods to compute SHA-256 hashes with optional salting.
    /// Supports output as raw bytes, Base64, or hexadecimal strings.
    /// </summary>
    public static class Sha256Hasher
    {
        /// <summary>
        /// Computes a SHA-256 hash of the input data, optionally combined with a salt.
        /// </summary>
        /// <param name="data">The input data to hash. Cannot be null.</param>
        /// <param name="salt">Optional salt appended to the data before hashing.</param>
        /// <returns>A byte array containing the computed SHA-256 hash.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        public static byte[] ComputeBytes(byte[] data, byte[]? salt = null)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            byte[] input = BuildInput(data, salt);
            return ComputeHash(input);
        }

        /// <summary>
        /// Computes a SHA-256 hash with optional salt and returns it as a hexadecimal string.
        /// </summary>
        /// <param name="data">The input data to hash. Cannot be null.</param>
        /// <param name="salt">Optional salt appended to the data before hashing.</param>
        /// <returns>An uppercase hexadecimal string representing the computed SHA-256 hash.</returns>
        public static string ComputeHex(byte[] data, byte[]? salt = null)
            => Convert.ToHexString(ComputeBytes(data, salt));

        /// <summary>
        /// Computes a SHA-256 hash with optional salt and returns it as a Base64-encoded string.
        /// </summary>
        /// <param name="data">The input data to hash. Cannot be null.</param>
        /// <param name="salt">Optional salt appended to the data before hashing.</param>
        /// <returns>A Base64-encoded string representing the computed SHA-256 hash.</returns>
        public static string ComputeBase64(byte[] data, byte[]? salt = null)
            => Convert.ToBase64String(ComputeBytes(data, salt));

        /// <summary>
        /// Combines data and optional salt into a single input buffer.
        /// </summary>
        /// <param name="data">The primary input data.</param>
        /// <param name="salt">Optional salt to append.</param>
        /// <returns>The combined byte array, or the original data if no salt is provided.</returns>
        private static byte[] BuildInput(byte[] data, byte[]? salt)
            => salt is null ? data : data.Concat(salt).ToArray();

        /// <summary>
        /// Computes the SHA-256 hash of the provided input buffer.
        /// </summary>
        /// <param name="input">The raw bytes to hash.</param>
        /// <returns>A byte array containing the SHA-256 digest.</returns>
        private static byte[] ComputeHash(byte[] input)
        {
            var digest = new Sha256Digest();
            digest.BlockUpdate(input, 0, input.Length);

            byte[] hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);
            return hash;
        }
    }
}