using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace BlockSense.Contracts.Cryptography.Utilities
{
    /// <summary>
    /// Provides utility methods for generating cryptographically secure random values.
    /// </summary>
    public static class CryptographyUtilities
    {
        /// <summary>
        /// Thread-safe, cryptographically secure random number generator backed by the OS crypto API.
        /// </summary>
        private static readonly SecureRandom SecureRandom =
            new SecureRandom(new CryptoApiRandomGenerator());

        /// <summary>
        /// Generates a cryptographically secure random byte array of the specified length.
        /// </summary>
        /// <param name="length">The number of random bytes to generate. Defaults to 16.</param>
        /// <returns>A byte array containing cryptographically secure random values.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is less than or equal to zero.</exception>
        public static byte[] GenerateSecureRandomBytes(int length = 16)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");

            byte[] randomBytes = new byte[length];
            SecureRandom.NextBytes(randomBytes);
            return randomBytes;
        }

        /// <summary>
        /// Generates a cryptographically secure random byte array and returns it as a Base64-encoded string.
        /// </summary>
        /// <param name="length">The number of random bytes to generate before encoding. Defaults to 16.</param>
        /// <returns>A Base64-encoded string representing the generated random bytes.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is less than or equal to zero.</exception>
        public static string GenerateSecureRandomBase64(int length = 16)
            => Convert.ToBase64String(GenerateSecureRandomBytes(length));
    }
}