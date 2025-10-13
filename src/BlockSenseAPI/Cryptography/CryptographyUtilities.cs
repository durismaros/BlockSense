using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace BlockSenseAPI.Cryptography
{
    /// <summary>
    /// Provides utility methods for generating cryptographically secure random values.
    /// </summary>
    public static class CryptographyUtilities
    {
        private static readonly SecureRandom _secureRandom =
            new SecureRandom(new CryptoApiRandomGenerator());

        /// <summary>
        /// Generates a cryptographically secure random byte array.
        /// </summary>
        /// <param name="length">Number of bytes to generate. Defaults to <c>16</c> if not specified.</param>
        /// <returns>A byte array filled with cryptographically secure random values.</returns>
        /// <exception cref="ArgumentOutOfRangeException">"Thrown when <paramref name="length"/> is less than or equal to zero"</exception>
        public static byte[] GenerateSecureRandomBytes(int length = 16)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            byte[] randomBytes = new byte[length];

            _secureRandom.NextBytes(randomBytes); // Fill the byte array with random values
            return randomBytes;
        }


        /// <summary>
        /// Generates a cryptographically secure random string encoded in Base64.
        /// </summary>
        /// <param name="length">Number of bytes to generate. Defaults to <c>16</c> if not specified.</param>
        /// <returns>A Base64-encoded string representing cryptographically secure random data.</returns>
        public static string GenerateSecureRandomBase64(int length = 16) => Convert.ToBase64String(GenerateSecureRandomBytes(length));
    }
}
