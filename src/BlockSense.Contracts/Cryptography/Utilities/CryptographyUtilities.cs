using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace BlockSense.Contracts.Cryptography.Utilities
{
    /// <summary>
    /// Utility class for cryptographic operations, specifically for generating cryptographically secure random values.
    /// </summary>
    public static class CryptographyUtilities
    {
        // Thread-safe, cryptographically secure random number generator
        private static readonly SecureRandom _secureRandom =
            new SecureRandom(new CryptoApiRandomGenerator());

        /// <summary>
        /// Generates a cryptographically secure random byte array.
        /// </summary>
        /// <param name="length">The number of random bytes to generate. Defaults to <c>16</c> if not specified.</param>
        /// <returns>A byte array containing cryptographically secure random values.</returns>
        /// <exception cref="ArgumentOutOfRangeException">"Thrown when <paramref name="length"/> is less than or equal to zero."</exception>
        public static byte[] GenerateSecureRandomBytes(int length = 16)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

            byte[] randomBytes = new byte[length];

            // Fill the byte array with random values
            _secureRandom.NextBytes(randomBytes);
            return randomBytes;
        }


        /// <summary>
        /// Generates a cryptographically secure random string, Base64-encoded.
        /// </summary>
        /// <param name="length">The number of random bytes to generate before encoding. Defaults to <c>16</c>.</param>
        /// <returns>A Base64-encoded string representing cryptographically secure random data.</returns>
        public static string GenerateSecureRandomBase64(int length = 16)
            => Convert.ToBase64String(GenerateSecureRandomBytes(length));
    }
}
