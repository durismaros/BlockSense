using BlockSense.Cryptography;
using BlockSenseAPI.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Cryptography.Encryption
{
    class Aes256Gcm
    {
        private const int KeySize = 32; // 256 bits
        private const int NonceSize = 12; // 96 bits (standard for GCM)
        private const int TagSize = 16; // 128 bits authentication tag

        /// <summary>
        /// Generates a cryptographically secure random nonce for AES-GCM.
        /// </summary>
        /// <returns>A 12-byte (96-bit) random nonce.</returns>
        public static byte[] GenerateNonce()
        {
            return CryptographyUtils.SecureRandomGenerator(NonceSize);
        }

        /// <summary>
        /// Encrypts the plaintext using AES-256-GCM.
        /// </summary>
        /// <param name="key">The 256-bit encryption key.</param>
        /// <param name="nonce">The 96-bit nonce.</param>
        /// <param name="plaintext">The data to encrypt.</param>
        /// <returns>The ciphertext with authentication tag appended.</returns>
        public static byte[] Encrypt(byte[] plaintext, byte[] key, byte[] nonce)
        {
            ValidateInputParameters(key, nonce, plaintext);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce);
            cipher.Init(true, parameters);

            var ciphertext = new byte[cipher.GetOutputSize(plaintext.Length)];
            var len = cipher.ProcessBytes(plaintext, 0, plaintext.Length, ciphertext, 0);
            cipher.DoFinal(ciphertext, len);

            return ciphertext;
        }

        /// <summary>
        /// Decrypts the ciphertext using AES-256-GCM.
        /// </summary>
        /// <param name="key">The 256-bit encryption key.</param>
        /// <param name="nonce">The 96-bit nonce.</param>
        /// <param name="ciphertextWithTag">The ciphertext with appended authentication tag.</param>
        /// <returns>The decrypted plaintext if authentication succeeds.</returns>
        public static byte[] Decrypt(byte[] ciphertextWithTag, byte[] key, byte[] nonce)
        {
            ValidateInputParameters(key, nonce, ciphertextWithTag);

            // Check if ciphertext is at least as long as the tag
            if (ciphertextWithTag.Length < TagSize)
            {
                throw new ArgumentException("Ciphertext must include the authentication tag and must be at least 16 bytes long.", nameof(ciphertextWithTag));
            }

            try
            {
                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce);
                cipher.Init(false, parameters);

                var plaintext = new byte[cipher.GetOutputSize(ciphertextWithTag.Length)];
                var len = cipher.ProcessBytes(ciphertextWithTag, 0, ciphertextWithTag.Length, plaintext, 0);
                cipher.DoFinal(plaintext, len);

                return plaintext;
            }
            catch (InvalidCipherTextException ex)
            {
                throw new CryptographicException("Failed to decrypt data - authentication tag verification failed.", ex);
            }
        }

        private static void ValidateInputParameters(byte[] key, byte[] nonce, byte[] data)
        {
            if (key == null || key.Length != KeySize)
                throw new ArgumentException($"Key must be {KeySize} bytes", nameof(key));
            if (nonce == null || nonce.Length != NonceSize)
                throw new ArgumentException($"Nonce must be {NonceSize} bytes", nameof(nonce));
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data cannot be empty", nameof(data));
        }
    }
}
