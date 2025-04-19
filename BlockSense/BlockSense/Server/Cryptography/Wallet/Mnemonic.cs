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


        public static List<string> MnemonicWords { get; } = GenerateMnemonic(WalletEntropyManager.Generate128bit());

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
