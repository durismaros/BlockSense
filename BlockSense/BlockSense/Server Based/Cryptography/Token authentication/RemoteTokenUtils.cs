using BlockSense.DB;
using CredentialManagement;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Server_Based.Cryptography
{
    class RemoteTokenUtils
    {
        public static byte[] ProcessBlocks(byte[] inputData, CbcBlockCipher blockCipher, bool isEncryption)
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


        public static string GenerateIV()
        {
            // Create an instance of SecureRandom from Bouncy Castle
            SecureRandom random = new SecureRandom();

            // AES uses a 128-bit IV (16 bytes)
            byte[] iv = new byte[16];

            // Fill the byte array with random values
            random.NextBytes(iv);

            string initializationVector = Convert.ToBase64String(iv);

            return initializationVector;
        }
    }
}
