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
        /// Size of the AES-256 key in bytes (32 bytes = 256 bits).
        /// </summary>
        private const int KeySize = 32;

        /// <summary>
        /// Recommended size of the Initialization Vector (IV) for AES-GCM in bytes (12 bytes = 96 bits).
        /// </summary>
        private const int IvSize = 12;

        /// <summary>
        /// Size of the authentication tag in bytes (16 bytes = 128 bits), used to verify data integrity.
        /// </summary>
        private const int TagSize = 16;

        /// <summary>
        /// Encrypts the provided plaintext using AES-256-GCM with authentication.
        /// </summary>
        /// <param name="key">The 32-byte encryption key.</param>
        /// <param name="iv">The 12-byte initialization vector.</param>
        /// <param name="plainText">The plaintext data to encrypt.</param>
        /// <returns>The ciphertext including the authentication tag.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/>, <paramref name="iv"/>, or <paramref name="plainText"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> or <paramref name="iv"/> lengths are invalid.</exception>
        public byte[] Encrypt(byte[] key, byte[] iv, byte[] plainText)
        {
            ValidateParameters(key, iv, plainText);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, iv);

            cipher.Init(true, parameters);

            var output = new byte[cipher.GetOutputSize(plainText.Length)];
            int len = cipher.ProcessBytes(plainText, 0, plainText.Length, output, 0);
            cipher.DoFinal(output, len);

            return output;
        }

        /// <summary>
        /// Decrypts the provided ciphertext using AES-256-GCM with authentication verification.
        /// </summary>
        /// <param name="key">The 32-byte encryption key.</param>
        /// <param name="iv">The 12-byte initialization vector.</param>
        /// <param name="cipherText">The ciphertext including the authentication tag.</param>
        /// <returns>The decrypted plaintext.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/>, <paramref name="iv"/>, or <paramref name="cipherText"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> or <paramref name="iv"/> lengths are invalid.</exception>
        public byte[] Decrypt(byte[] key, byte[] iv, byte[] cipherText)
        {
            ValidateParameters(key, iv, cipherText);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, iv);

            cipher.Init(false, parameters);

            var output = new byte[cipher.GetOutputSize(cipherText.Length)];
            int len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, output, 0);
            cipher.DoFinal(output, len);

            return output;
        }

        /// <summary>
        /// Validates key, IV, and data for AES-256-GCM operations.
        /// </summary>
        /// <param name="key">Encryption key.</param>
        /// <param name="iv">Initialization vector.</param>
        /// <param name="data">Data to process (plaintext or ciphertext).</param>
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
