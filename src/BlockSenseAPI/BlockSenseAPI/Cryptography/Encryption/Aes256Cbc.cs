using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Security.Cryptography;

namespace BlockSense.Cryptography.Encryption
{
    class Encryption
    {
        private const int _keySize = 256; // 256-bit
        private const int _blockSize = 128; // block size in bits
        private const int _ivSize = 16; // 128 bits for IV

        public byte[] Encrypt(byte[] plaintext, byte[] key, byte[] iv)
        {
            ValidateKeyAndIv(key, iv);

            try
            {
                // Create cipher
                IBufferedCipher cipher = CreateCipher(true, key, iv);

                // Encrypt the plaintext
                return cipher.DoFinal(plaintext);
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Encryption failed", ex);
            }
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] key, byte[] iv)
        {
            ValidateKeyAndIv(key, iv);

            try
            {
                // Create cipher
                IBufferedCipher cipher = CreateCipher(false, key, iv);

                // Decrypt the ciphertext
                return cipher.DoFinal(ciphertext);
            }
            catch (Exception ex)
            {
                throw new CryptographicException("Decryption failed", ex);
            }
        }

        private static void ValidateKeyAndIv(byte[] key, byte[] iv)
        {
            if (key == null || key.Length != _keySize / 8)
            {
                throw new ArgumentException($"Key must be {_keySize} bits long (got {key?.Length * 8 ?? 0} bits)", nameof(key));
            }

            if (iv == null || iv.Length != _ivSize)
            {
                throw new ArgumentException($"IV must be {_blockSize} bits long (got {iv?.Length * 8 ?? 0} bits)", nameof(iv));
            }
        }

        private static IBufferedCipher CreateCipher(bool forEncryption, byte[] key, byte[] iv)
        {
            // Create AES engine with CBC mode and PKCS7 padding
            var aesEngine = new AesEngine();
            var cbcBlockCipher = new CbcBlockCipher(aesEngine);
            var paddedCipher = new PaddedBufferedBlockCipher(cbcBlockCipher, new Pkcs7Padding());

            // Initialize cipher with key and IV
            KeyParameter keyParam = new KeyParameter(key);
            ParametersWithIV keyParamWithIv = new ParametersWithIV(keyParam, iv);
            paddedCipher.Init(forEncryption, keyParamWithIv);

            return paddedCipher;
        }
    }
}
