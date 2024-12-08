using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;

namespace BlockSense
{
    internal class Hash
    {
        public static string GenerateSalt(int length = 16)
        {
            // Generates a random salt using BouncyCastle
            var random = new Org.BouncyCastle.Crypto.Prng.VmpcRandomGenerator();
            byte[] salt = new byte[length];
            random.NextBytes(salt);
            return Convert.ToBase64String(salt);
        }

        public static string HashPassword(string password, string salt)
        {
            // Combine password and salt
            var passwordBytes = Encoding.UTF8.GetBytes(password + salt);

            // Create a SHA256 instance from BouncyCastle
            var sha256Digest = new Sha256Digest();
            sha256Digest.BlockUpdate(passwordBytes, 0, passwordBytes.Length);

            // Compute the hash
            byte[] hash = new byte[sha256Digest.GetDigestSize()];
            sha256Digest.DoFinal(hash, 0);

            // Return the hashed password in Base64 format
            return Convert.ToBase64String(hash);
        }


    }
}
