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
    class TokenEncryption
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
            byte[] encryptedTokenBytes = RemoteTokenUtils.ProcessBlocks(refreshTokenBytes, blockCipher, true);

            string encryptedToken = Convert.ToBase64String(encryptedTokenBytes);

            // Return the encrypted token as a Base64 string
            return encryptedToken;
        }
    }
}
