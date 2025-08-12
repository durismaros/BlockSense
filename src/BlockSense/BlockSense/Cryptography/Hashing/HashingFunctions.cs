using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Prng;
using System.Linq;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace BlockSense.Cryptography.Hashing
{
    class HashingFunctions
    {
        /// <summary>
        /// Hashes a string with/without salt with SHA256 algorithm using BouncyCastle library
        /// </summary>
        /// <param name="data">data to hash</param>
        /// <param name="salt">additional salt to make the hash unique</param>
        /// <returns>SHA256 hash as a byte array</returns>
        public static byte[] ComputeSha256(byte[] data, byte[]? salt = null)
        {
            if (salt != null) data = data.Concat(salt).ToArray();

            // Create a SHA256 instance from BouncyCastle
            var sha256Digest = new Sha256Digest();
            sha256Digest.BlockUpdate(data, 0, data.Length);

            // Compute the hash
            byte[] hash = new byte[sha256Digest.GetDigestSize()];
            sha256Digest.DoFinal(hash, 0);

            // Return the hashed password in Base64 format
            return hash;
        }

        /// <summary>
        /// Computes HMAC-SHA256 hash of the input data using the provided key
        /// </summary>
        /// <param name="data">data to hash</param>
        /// <param name="key">the secret key for HMAC</param>
        /// <returns>HMAC-SHA256 hash as a byte array</returns>
        public static byte[] ComputeHmacSha256(byte[] data, byte[] key)
        {
            var hmac = new HMac(new Sha256Digest());
            hmac.Init(new KeyParameter(key));

            byte[] result = new byte[hmac.GetMacSize()];
            hmac.BlockUpdate(data, 0, data.Length);
            hmac.DoFinal(result, 0);

            return result;
        }
    }
}
