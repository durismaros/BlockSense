using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Client.Cryptography
{
    class CryptoUtils
    {
        /// <summary>
        /// Generates a secure random array of bytes using BouncyCastle library
        /// </summary>
        /// <param name="length">default - 16 bytes</param>
        /// <returns>byte array filled with random bytes</returns>
        public static byte[] SecureRandomGenerator(int length = 16)
        {
            // Create an instance of SecureRandom from Bouncy Castle
            SecureRandom random = new SecureRandom(new CryptoApiRandomGenerator());

            byte[] randomBytes = new byte[length];

            // Fill the byte array with random values
            random.NextBytes(randomBytes);

            return randomBytes;
        }
    }
}
