using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockSense.Contracts.Cryptography.Encryption
{
    /// <summary>
    /// Provides AES-256 encryption and decryption using GCM mode with authenticated data integrity.
    /// Supports 256-bit keys, 12-byte IVs, and 128-bit authentication tags.
    /// </summary>
    public sealed class Aes256GcmEncryptor
    {
        /// <summary>
        /// Required AES-256 key size in bytes (32 bytes = 256 bits).
        /// </summary>
        private const int KeySize = 32;

        /// <summary>
        /// Recommended IV size in bytes for AES-GCM (12 bytes = 96 bits).
        /// </summary>
        private const int IvSize = 12;

        /// <summary>
        /// Authentication tag size in bytes (16 bytes = 128 bits).
        /// </summary>
        private const int TagSize = 16;

        /// <summary>
        /// Encrypts the provided plaintext using AES-256-GCM with authentication.
        /// </summary>
        /// <param name="key">The 32-byte encryption key.</param>
        /// <param name="iv">The 12-byte initialization vector.</param>
        /// <param name="plainText">The plaintext data to encrypt.</param>
        /// <returns>The ciphertext including the appended authentication tag.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/>, <paramref name="iv"/>, or <paramref name="plainText"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> or <paramref name="iv"/> lengths are invalid.</exception>
        public byte[] Encrypt(byte[] key, byte[] iv, byte[] plainText)
        {
            ValidateParameters(key, iv, plainText);

            var cipher = InitializeCipher(key, iv, forEncryption: true);
            return ProcessData(cipher, plainText);
        }

        /// <summary>
        /// Decrypts the provided ciphertext using AES-256-GCM with authentication verification.
        /// </summary>
        /// <param name="key">The 32-byte encryption key.</param>
        /// <param name="iv">The 12-byte initialization vector.</param>
        /// <param name="cipherText">The ciphertext including the authentication tag.</param>
        /// <returns>The decrypted plaintext.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/>, <paramref name="iv"/>, or <paramref name="cipherText"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> or <paramref name="iv"/> lengths are invalid.</exception>
        public byte[] Decrypt(byte[] key, byte[] iv, byte[] cipherText)
        {
            ValidateParameters(key, iv, cipherText);

            var cipher = InitializeCipher(key, iv, forEncryption: false);
            return ProcessData(cipher, cipherText);
        }

        /// <summary>
        /// Initializes and configures an AES-GCM cipher instance for encryption or decryption.
        /// </summary>
        /// <param name="key">The 32-byte encryption key.</param>
        /// <param name="iv">The 12-byte initialization vector.</param>
        /// <param name="forEncryption">True to configure for encryption; false for decryption.</param>
        /// <returns>An initialized <see cref="GcmBlockCipher"/> ready for data processing.</returns>
        private static GcmBlockCipher InitializeCipher(byte[] key, byte[] iv, bool forEncryption)
        {
            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, iv);
            cipher.Init(forEncryption, parameters);
            return cipher;
        }

        /// <summary>
        /// Processes the input data through the cipher and returns the result.
        /// </summary>
        /// <param name="cipher">The initialized GCM cipher.</param>
        /// <param name="input">The data to process (plaintext or ciphertext).</param>
        /// <returns>The processed output as a byte array.</returns>
        private static byte[] ProcessData(GcmBlockCipher cipher, byte[] input)
        {
            byte[] output = new byte[cipher.GetOutputSize(input.Length)];
            int length = cipher.ProcessBytes(input, 0, input.Length, output, 0);
            cipher.DoFinal(output, length);
            return output;
        }

        /// <summary>
        /// Validates key, IV, and data parameters for AES-256-GCM operations.
        /// </summary>
        /// <param name="key">The encryption key to validate.</param>
        /// <param name="iv">The initialization vector to validate.</param>
        /// <param name="data">The data to validate (plaintext or ciphertext).</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when key or IV lengths are invalid.</exception>
        private static void ValidateParameters(byte[] key, byte[] iv, byte[] data)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if (iv is null) throw new ArgumentNullException(nameof(iv));
            if (data is null) throw new ArgumentNullException(nameof(data));

            if (key.Length != KeySize)
                throw new ArgumentException($"AES-256 key must be {KeySize} bytes.", nameof(key));

            if (iv.Length != IvSize)
                throw new ArgumentException($"AES-GCM IV must be {IvSize} bytes.", nameof(iv));
        }
    }
}