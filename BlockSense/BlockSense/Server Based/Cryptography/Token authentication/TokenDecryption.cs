using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Server_Based.Cryptography.Tokens
{
    class TokenDecryption
    {
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
            byte[] decryptedToken = RemoteTokenUtils.ProcessBlocks(encryptedTokenBytes, blockCipher, false);

            // Return the decrypted token as a Base64 string
            return Convert.ToBase64String(decryptedToken);
        }
    }
}
