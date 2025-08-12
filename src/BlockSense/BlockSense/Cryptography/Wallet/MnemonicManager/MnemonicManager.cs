using BlockSense.auth.DataProtection;
using BlockSense.Cryptography;
using BlockSense.Cryptography.Encryption;
using BlockSense.Cryptography.KeyDerivation;
using BlockSense.Cryptography.Wallet;
using BlockSense.Utilities;
using NBitcoin.WalletPolicies;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace BlockSense.Cryptography.Wallet.MnemonicManager
{
    class MnemonicManager
    {
        private static string WalletPath { get; }
        private static string LevelDbPath { get; }

        private const int SaltSize = 16; // 128-bit salt for Argon2id
        private const int PinAttemptLimit = 10;
        private static int _failedPinAttempts = 0;
        private static readonly LevelDbStorage _db;
        public static readonly string[] BIP39_WORDLIST = File.ReadAllLines(@"C:\Users\d3str\Desktop\School\BlockSense\BlockSense\BlockSense\Assets\bip39_english.txt");
        public static List<string> MnemonicWords { get; } = GenerateMnemonic(WalletEntropyManager.GenerateEntropy());

        static MnemonicManager()
        {
            WalletPath = DirectoryStructure.WalletPath;
            LevelDbPath = Path.Combine(WalletPath, "leveldb");

            _db = new LevelDbStorage(LevelDbPath);
        }

        /// <summary>
        /// Initializes a new wallet by generating and storing a secure salt
        /// </summary>
        public static byte[] InitializeWallet()
        {
            byte[] salt = CryptographyUtils.SecureRandomGenerator(SaltSize);
            _db.Put("wallet:salt", salt);
            return salt;
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

        public static void StoreMnemonic(List<string> mnemonicList, string pin)
        {
            if (mnemonicList == null || mnemonicList.Count != 12)
                throw new ArgumentException("Mnemonic must be 12 words", nameof(mnemonicList));
            if (string.IsNullOrWhiteSpace(pin))
                throw new ArgumentException("PIN cannot be empty", nameof(pin));

            string mnemonic = string.Join(" ", mnemonicList);

            // Get or create salt
            byte[] salt = _db.Get("wallet:salt") ?? InitializeWallet();

            // Derive encryption key from user's PIN
            byte[] key = Argon2IdDerivation.DeriveKeyFromPin(pin, salt);

            try
            {
                byte[] plainText = Encoding.UTF8.GetBytes(mnemonic);
                byte[] nonce = Aes256Gcm.GenerateNonce();
                byte[] cipherText = Aes256Gcm.Encrypt(plainText, key, nonce);

                // Store as concatenated binary: nonce(12) + ciphertext_with_tag(variable)
                var storageFormat = new byte[nonce.Length + cipherText.Length];
                Buffer.BlockCopy(nonce, 0, storageFormat, 0, nonce.Length);
                Buffer.BlockCopy(cipherText, 0, storageFormat, nonce.Length, cipherText.Length);

                _db.Put("wallet:mnemonic", storageFormat);
            }
            finally
            {
                SecureWiper.Wipe(ref key);
            }
        }

        public static List<string> RetrieveMnemonic(string pin)
        {
            if (_failedPinAttempts >= PinAttemptLimit)
            {
                WipeWallet();
                throw new SecurityException("Maximum PIN attempts reached. Wallet wiped.");
            }

            byte[] salt = _db.Get("wallet:salt") ?? throw new InvalidOperationException("Wallet not initialized");
            byte[] storedData = _db.Get("wallet:mnemonic") ?? throw new InvalidOperationException("No mnemonic found");

            byte[] key = Argon2IdDerivation.DeriveKeyFromPin(pin, salt);

            try
            {
                // Parse stored binary format
                byte[] nonce = new byte[12];
                byte[] ciphertext = new byte[storedData.Length - 12];

                Buffer.BlockCopy(storedData, 0, nonce, 0, 12);
                Buffer.BlockCopy(storedData, 12, ciphertext, 0, ciphertext.Length);

                byte[] decrypted = Aes256Gcm.Decrypt(ciphertext, key, nonce);
                _failedPinAttempts = 0;

                string mnemonic = Encoding.UTF8.GetString(decrypted);
                var words = mnemonic.Split(' ');

                if (words.Length != 12)
                    throw new CryptographicException("Decrypted mnemonic is invalid");

                return new List<string>(words);
            }
            catch (CryptographicException)
            {
                _failedPinAttempts++;
                if (_failedPinAttempts >= PinAttemptLimit) WipeWallet();
                throw new SecurityException("Invalid PIN");
            }
            finally
            {
                SecureWiper.Wipe(ref key);
            }
        }

        public static void WipeWallet()
        {
            _db.Delete("wallet:salt");
            _db.Delete("wallet:mnemonic");
            _failedPinAttempts = 0;
        }
    }
}
