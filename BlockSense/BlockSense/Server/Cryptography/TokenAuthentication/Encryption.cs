using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;

namespace BlockSense.Server.Cryptography.TokenAuthentication
{
    class Encryption
    {
        public static string EncryptRefreshToken(string refreshToken, string encryptionkey, string iv)
        {
            byte[] refreshTokenBytes = Convert.FromBase64String(refreshToken);
            byte[] encryptionkeyBytes = Convert.FromBase64String(encryptionkey);
            byte[] ivBytes = Convert.FromBase64String(iv);

            // Set up the AES cipher for encryption (AES-256 in CBC mode)
            var cipher = new AesEngine(); // AES engine
            var blockCipher = new CbcBlockCipher(cipher); // CBC mode
            var padding = new Pkcs7Padding(); // Apply PKCS7 padding
            var cipherParams = new ParametersWithIV(new KeyParameter(encryptionkeyBytes), ivBytes); // AES key + IV

            // Initialize the cipher for encryption
            blockCipher.Init(true, cipherParams);

            // Encrypt the token (do block-wise encryption)
            byte[] encryptedTokenBytes = ProcessBlocks(refreshTokenBytes, blockCipher, false);

            string encryptedToken = Convert.ToBase64String(encryptedTokenBytes);

            // Return the encrypted token as a Base64 string
            return encryptedToken;
        }

        public static string DecryptRefreshToken(string encryptedToken, string encryptionkey, string iv)
        {
            // Convert the Base64-encoded encrypted token back to bytes
            byte[] encryptedTokenBytes = Convert.FromBase64String(encryptedToken);
            byte[] encryptionKeyBytes = Convert.FromBase64String(encryptionkey);
            byte[] ivBytes = Convert.FromBase64String(iv);

            // Set up the AES cipher for decryption (AES-256 in CBC mode)
            var cipher = new AesEngine(); // AES engine
            var blockCipher = new CbcBlockCipher(cipher); // CBC mode
            var padding = new Pkcs7Padding(); // Apply PKCS7 padding
            var cipherParams = new ParametersWithIV(new KeyParameter(encryptionKeyBytes), ivBytes); // AES key + IV

            // Initialize the cipher for decryption
            blockCipher.Init(false, cipherParams); // false for decryption

            // Decrypt the token (do block-wise decryption)
            byte[] decryptedToken = ProcessBlocks(encryptedTokenBytes, blockCipher, false);

            // Return the decrypted token as a Base64 string
            return Convert.ToBase64String(decryptedToken);
        }

        private static byte[] ProcessBlocks(byte[] inputData, CbcBlockCipher blockCipher, bool isEncryption)
        {
            int blockSize = blockCipher.GetBlockSize();
            int inputLength = inputData.Length, outputLength = inputData.Length;

            // Allocate space for the output (either encrypted or decrypted data)
            byte[] outputData = new byte[outputLength];

            // Process input in blocks
            for (int i = 0; i < inputLength; i += blockSize)
            {
                blockCipher.ProcessBlock(inputData, i, outputData, i);
            }

            return outputData;
        }
    }
}
