using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockSenseAPI.Cryptography.Encryption
{
    /// <summary>
    /// Provides AES encryption and decryption using CBC mode with 256-bit keys and PKCS7 padding.
    /// </summary>
    public sealed class Aes256CbcEncryptor
    {
        /// <summary>
        /// The size of the AES-256 key in bytes (32 bytes = 256 bits).
        /// </summary>
        private const int KeySize = 32;

        /// <summary>
        /// The recommended size of the Initialization Vector (IV) for AES-CBC in bytes (16 bytes = 128 bits).
        /// </summary>
        private const int IvSize = 16;

        /// <summary>
        /// Encrypts the provided plainText using AES-256-CBC with the specified key and IV.
        /// </summary>
        /// <param name="key">The 32-byte encryption key.</param>
        /// <param name="iv">The 16-byte initialization vector.</param>
        /// <param name="plainText">The data to encrypt.</param>
        /// <returns>The encrypted data as a byte array.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/>, <paramref name="iv"/>, or <paramref name="plainText"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> or <paramref name="iv"/> lengths are invalid.</exception>
        public byte[] Encrypt(byte[] key, byte[] iv, byte[] plainText)
        {
            ValidateParameters(key, iv, plainText);

            var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine()), new Pkcs7Padding());
            var parameters = new ParametersWithIV(new KeyParameter(key), iv);

            cipher.Init(true, parameters);

            return cipher.DoFinal(plainText);
        }

        /// <summary>
        /// Decrypts the provided cipherText using AES-256-CBC with the specified key and IV.
        /// </summary>
        /// <param name="key">The 32-byte encryption key.</param>
        /// <param name="iv">The 16-byte initialization vector.</param>
        /// <param name="cipherText">The encrypted data to decrypt.</param>
        /// <returns>The decrypted data as a byte array.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/>, <paramref name="iv"/>, or <paramref name="plainText"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> or <paramref name="iv"/> lengths are invalid.</exception>
        public byte[] Decrypt(byte[] key, byte[] iv, byte[] cipherText)
        {
            ValidateParameters(key, iv, cipherText);

            var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine()), new Pkcs7Padding());
            var parameters = new ParametersWithIV(new KeyParameter(key), iv);

            cipher.Init(false, parameters);

            return cipher.DoFinal(cipherText);
        }

        /// <summary>
        /// Validates key, IV, and data for AES-256-CBC operations.
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
                throw new ArgumentException($"AES IV must be {IvSize} bytes.", nameof(iv));
        }
    }
}
