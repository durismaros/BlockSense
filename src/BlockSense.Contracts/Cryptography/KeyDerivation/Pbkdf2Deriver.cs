using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockSense.Contracts.Cryptography.KeyDerivation
{
    /// <summary>
    /// Provides PBKDF2 (Password-Based Key Derivation Function 2) key derivation using HMAC-SHA256.
    /// Suitable for deriving cryptographic keys from passwords or passphrases with configurable
    /// iteration counts, salt, and output lengths. Compliant with RFC 2898 / PKCS#5 v2.0.
    /// </summary>
    public static class Pbkdf2Deriver
    {
        /// <summary>
        /// Default number of iterations recommended for PBKDF2-HMAC-SHA256 (OWASP 2023 guideline).
        /// Higher values increase resistance to brute-force attacks at the cost of computation time.
        /// </summary>
        public const int DefaultIterations = 600_000;

        /// <summary>
        /// Recommended salt size in bytes (16 bytes = 128 bits), sufficient for collision resistance.
        /// </summary>
        public const int RecommendedSaltSize = 16;

        /// <summary>
        /// Default derived key length in bytes (32 bytes = 256 bits), suitable for AES-256.
        /// </summary>
        public const int DefaultKeySize = 32;

        /// <summary>
        /// Derives a cryptographic key from the provided password and salt using PBKDF2-HMAC-SHA256.
        /// </summary>
        /// <param name="password">The password bytes to derive a key from. Cannot be null or empty.</param>
        /// <param name="salt">
        /// The cryptographic salt. Must be at least 8 bytes. Use a random, unique salt per credential.
        /// Cannot be null.
        /// </param>
        /// <param name="iterations">
        /// The number of PBKDF2 iterations. Defaults to <see cref="DefaultIterations"/>.
        /// Must be greater than zero. Higher values improve security but increase computation time.
        /// </param>
        /// <param name="keySize">
        /// The desired output key length in bytes. Defaults to <see cref="DefaultKeySize"/> (32 bytes).
        /// Must be greater than zero.
        /// </param>
        /// <returns>A byte array of length <paramref name="keySize"/> containing the derived key.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="password"/> or <paramref name="salt"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="password"/> is empty, <paramref name="salt"/> is shorter than
        /// 8 bytes, <paramref name="iterations"/> is less than or equal to zero, or
        /// <paramref name="keySize"/> is less than or equal to zero.
        /// </exception>
        public static byte[] DeriveBytes(
            byte[] password,
            byte[] salt,
            int iterations = DefaultIterations,
            int keySize = DefaultKeySize)
        {
            ValidateParameters(password, salt, iterations, keySize);

            var generator = new Pkcs5S2ParametersGenerator(new Sha256Digest());
            generator.Init(password, salt, iterations);

            var keyParameter = (KeyParameter)generator.GenerateDerivedParameters("AES", keySize * 8);
            return keyParameter.GetKey();
        }

        /// <summary>
        /// Derives a cryptographic key from the provided password and salt using PBKDF2-HMAC-SHA256
        /// and returns it as a hexadecimal string.
        /// </summary>
        /// <param name="password">The password bytes to derive a key from. Cannot be null or empty.</param>
        /// <param name="salt">
        /// The cryptographic salt. Must be at least 8 bytes. Cannot be null.
        /// </param>
        /// <param name="iterations">
        /// The number of PBKDF2 iterations. Defaults to <see cref="DefaultIterations"/>.
        /// </param>
        /// <param name="keySize">
        /// The desired output key length in bytes. Defaults to <see cref="DefaultKeySize"/>.
        /// </param>
        /// <returns>An uppercase hexadecimal string representing the derived key.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="password"/> or <paramref name="salt"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any parameter fails validation. See <see cref="DeriveBytes"/>.
        /// </exception>
        public static string DeriveHex(
            byte[] password,
            byte[] salt,
            int iterations = DefaultIterations,
            int keySize = DefaultKeySize)
            => Convert.ToHexString(DeriveBytes(password, salt, iterations, keySize));

        /// <summary>
        /// Derives a cryptographic key from the provided password and salt using PBKDF2-HMAC-SHA256
        /// and returns it as a Base64-encoded string.
        /// </summary>
        /// <param name="password">The password bytes to derive a key from. Cannot be null or empty.</param>
        /// <param name="salt">
        /// The cryptographic salt. Must be at least 8 bytes. Cannot be null.
        /// </param>
        /// <param name="iterations">
        /// The number of PBKDF2 iterations. Defaults to <see cref="DefaultIterations"/>.
        /// </param>
        /// <param name="keySize">
        /// The desired output key length in bytes. Defaults to <see cref="DefaultKeySize"/>.
        /// </param>
        /// <returns>A Base64-encoded string representing the derived key.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="password"/> or <paramref name="salt"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any parameter fails validation. See <see cref="DeriveBytes"/>.
        /// </exception>
        public static string DeriveBase64(
            byte[] password,
            byte[] salt,
            int iterations = DefaultIterations,
            int keySize = DefaultKeySize)
            => Convert.ToBase64String(DeriveBytes(password, salt, iterations, keySize));

        /// <summary>
        /// Validates all input parameters for PBKDF2 key derivation.
        /// </summary>
        /// <param name="password">The password bytes to validate.</param>
        /// <param name="salt">The salt bytes to validate.</param>
        /// <param name="iterations">The iteration count to validate.</param>
        /// <param name="keySize">The desired key size in bytes to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when password or salt is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any parameter is invalid.</exception>
        private static void ValidateParameters(byte[] password, byte[] salt, int iterations, int keySize)
        {
            if (password is null)
                throw new ArgumentNullException(nameof(password));

            if (salt is null)
                throw new ArgumentNullException(nameof(salt));

            if (password.Length == 0)
                throw new ArgumentException("Password must not be empty.", nameof(password));

            if (salt.Length < 8)
                throw new ArgumentException("Salt must be at least 8 bytes for PBKDF2 compliance.", nameof(salt));

            if (iterations <= 0)
                throw new ArgumentException("Iteration count must be greater than zero.", nameof(iterations));

            if (keySize <= 0)
                throw new ArgumentException("Key size must be greater than zero.", nameof(keySize));
        }
    }
}
