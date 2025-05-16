using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace BlockSenseAPI.Cryptography
{
    public class CryptographyUtils
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
