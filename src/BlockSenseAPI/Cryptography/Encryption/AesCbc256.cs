using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;

namespace BlockSense.Cryptography.Encryption
{
    class AesCbc256
    {
        private const int KeySize = 256; // 256-bit
        private const int BlockSize = 128; // block size in bits
        private const int IvSize = 16; // 128 bits for IV

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
            if (key == null || key.Length != KeySize / 8)
            {
                throw new ArgumentException($"Key must be {KeySize} bits long (got {key?.Length * 8 ?? 0} bits)", nameof(key));
            }

            if (iv == null || iv.Length != IvSize)
            {
                throw new ArgumentException($"IV must be {BlockSize} bits long (got {iv?.Length * 8 ?? 0} bits)", nameof(iv));
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
