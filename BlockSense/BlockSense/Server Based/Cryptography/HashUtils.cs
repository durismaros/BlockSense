using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using System.Collections.Generic;
using BlockSense.DB;
using System.Net.Quic;
using System.Data;
using System.Threading.Tasks;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.IO;
using Org.BouncyCastle.Crypto.Prng;

namespace BlockSense
{
    class HashUtils
    {


        public static string GenerateHash(string password, string? salt)
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


        public static string GenerateSalt(int length = 16)
        {
            // Generates a random salt using BouncyCastle
            var random = new VmpcRandomGenerator();
            byte[] salt = new byte[length];
            random.NextBytes(salt);
            return Convert.ToBase64String(salt);
        }
    }
}
