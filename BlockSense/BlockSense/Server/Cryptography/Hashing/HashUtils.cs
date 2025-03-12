using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Prng;

namespace BlockSense.Server.Cryptography.Hashing
{
    class HashUtils
    {
        /// <summary>
        /// Hashed string with/without salt with SHA256 algorithm using BouncyCastle library
        /// </summary>
        /// <param name="input">plain string</param>
        /// <param name="salt"></param>
        /// <returns>hashed string in Base64</returns>
        public static string GenerateHash(string input, string? salt)
        {
            // Combine input and salt
            var passwordBytes = Encoding.UTF8.GetBytes(input + salt);

            // Create a SHA256 instance from BouncyCastle
            var sha256Digest = new Sha256Digest();
            sha256Digest.BlockUpdate(passwordBytes, 0, passwordBytes.Length);

            // Compute the hash
            byte[] hash = new byte[sha256Digest.GetDigestSize()];
            sha256Digest.DoFinal(hash, 0);

            // Return the hashed password in Base64 format
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Generates a secure random array of bytes using BouncyCastle lib
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
