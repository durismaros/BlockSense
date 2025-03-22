using BlockSense.Server.Cryptography.Hashing;
using NBitcoin.BIP322;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.IO;

namespace BlockSense.Server.Cryptography.Wallet
{
    class Mnemonic
    {
        public static readonly string[] BIP39_WORDLIST = File.ReadAllLines(@"C:\Users\d3str\Desktop\School\BlockSense\BlockSense\BlockSense\Assets\bip39_english.txt");

        private static byte[] _sourceEntropy = HashUtils.SecureRandomGenerator();
        private static byte[] _finalEntropy = AppendChecksum(_sourceEntropy);
        public static List<string> MnemonicWords { get; } = GenerateMnemonic(_finalEntropy);


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

        private static List<string> GenerateMnemonic(byte[] entropyWithChecksum)
        {
            List<string> mnemonic = new();
            string binaryString = "";

            for (int i = 0; i < entropyWithChecksum.Length; i++)
            {
                byte b = entropyWithChecksum[i];

                // Pad all bytes except the last one
                if (i < entropyWithChecksum.Length - 1)
                    binaryString += Convert.ToString(b, 2).PadLeft(8, '0');

                else
                    binaryString += Convert.ToString(b, 2).PadLeft(4, '0');
            }

            for (int i = 0; i + 11 <= binaryString.Length; i += 11)
                mnemonic.Add(BIP39_WORDLIST[Convert.ToInt32(binaryString.Substring(i, 11), 2)]);

            return mnemonic;
        }
    }
}
