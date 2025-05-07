using BlockSense.Client.Cryptography;
using BlockSense.Client.Cryptography.Hashing;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Paddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Server.Cryptography.Wallet
{
    class WalletEntropyManager
    {
        private static byte[] AppendChecksum(byte[] entropySource)
        {
            // Create a SHA256 instance from BouncyCastle
            var sha256Digest = new Sha256Digest();
            sha256Digest.BlockUpdate(entropySource, 0, entropySource.Length);

            // Compute the hash
            byte[] hashedEntropy = new byte[sha256Digest.GetDigestSize()];
            sha256Digest.DoFinal(hashedEntropy, 0);

            byte checksum = (byte)(hashedEntropy[0] >> (8 - 4));

            byte[] verifiedEntropy = new byte[entropySource.Length + 1];

            // Combine entropy and checksum into one array
            Array.Copy(entropySource, verifiedEntropy, entropySource.Length);
            verifiedEntropy[entropySource.Length] = checksum;

            return verifiedEntropy;
        }

        public static byte[] Generate128bit()
        {
            byte[] sourceEntropy = CryptoUtils.SecureRandomGenerator();

            return AppendChecksum(sourceEntropy);
        }
    }
}
