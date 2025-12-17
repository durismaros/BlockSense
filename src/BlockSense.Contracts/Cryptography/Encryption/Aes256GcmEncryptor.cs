using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockSense.Contracts.Cryptography.Encryption
{
    /// <summary>
    /// Provides AES encryption and decryption using GCM mode with 256-bit keys and authenticated data integrity.
    /// </summary>
    public sealed class Aes256GcmEncryptor
    {
        /// <summary>
        /// The size of the AES-256 key in bytes (32 bytes = 256 bits).
        /// </summary>
        private const int KeySize = 32;

        /// <summary>
        /// The recommended size of the Initialization Vector (IV) for AES-GCM in bytes (12 bytes = 96 bits).
        /// </summary>
        private const int IvSize = 12;

        /// <summary>
        /// The size of the authentication tag in bytes (16 bytes = 128 bits), used to verify data integrity.
        /// </summary>
        private const int TagSize = 16;

        /// <summary>
        /// Encrypts the provided plainText using AES-256-GCM with the specified key and IV.
        /// </summary>
        /// <param name="key">The 32-byte encryption key.</param>
        /// <param name="iv">The 12-byte initialization vector.</param>
        /// <param name="plainText">The data to encrypt.</param>
        /// <returns>The encrypted data including the authentication tag as a byte array.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/>, <paramref name="iv"/> or <paramref name="plainText"/> is null.</exception>
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
        /// Decrypts the provided cipherText using AES-256-GCM with the specified key and IV.
        /// </summary>
        /// <param name="key">The 32-byte encryption key.</param>
        /// <param name="iv">The 12-byte initialization vector.</param>
        /// <param name="cipherText">The encrypted data including the authentication tag.</param>
        /// <returns>The decrypted data as a byte array.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/>, <paramref name="iv"/> or <paramref name="plainText"/> is null.</exception>
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
        /// <param name="data">Data to process.</param>
        private static void ValidateParameters(byte[] key, byte[] iv, byte[] data)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (iv == null)
                throw new ArgumentNullException(nameof(iv));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (key.Length != KeySize)
                throw new ArgumentException($"AES-256 key must be {KeySize} bytes.", nameof(key));
            if (iv.Length != IvSize)
                throw new ArgumentException($"AES-GCM IV must be {IvSize} bytes.", nameof(iv));
        }
    }
}
