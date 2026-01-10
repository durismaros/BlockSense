using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockSense.Contracts.Cryptography.Encryption
{
    /// <summary>
    /// Provides AES-256 encryption and decryption using CBC mode with PKCS7 padding.
    /// Designed for secure symmetric encryption with a 32-byte key and 16-byte IV.
    /// </summary>
    public sealed class Aes256CbcEncryptor
    {
        /// <summary>
        /// Size of the AES-256 key in bytes (32 bytes = 256 bits).
        /// </summary>
        private const int KeySize = 32;

        /// <summary>
        /// Recommended size of the Initialization Vector (IV) for AES-CBC in bytes (16 bytes = 128 bits).
        /// </summary>
        private const int IvSize = 16;

        /// <summary>
        /// Encrypts the provided plaintext using AES-256-CBC with PKCS7 padding.
        /// </summary>
        /// <param name="key">The 32-byte encryption key.</param>
        /// <param name="iv">The 16-byte initialization vector.</param>
        /// <param name="plainText">The plaintext data to encrypt.</param>
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
        /// Decrypts the provided ciphertext using AES-256-CBC with PKCS7 padding.
        /// </summary>
        /// <param name="key">The 32-byte encryption key.</param>
        /// <param name="iv">The 16-byte initialization vector.</param>
        /// <param name="cipherText">The encrypted data to decrypt.</param>
        /// <returns>The decrypted plaintext as a byte array.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/>, <paramref name="iv"/>, or <paramref name="cipherText"/> is null.</exception>
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
        /// Validates key, IV, and input data for AES-256-CBC operations.
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
                throw new ArgumentException($"AES IV must be {IvSize} bytes.", nameof(iv));
        }
    }
}
