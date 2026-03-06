using BlockSense.Contracts.Cryptography.Encryption;
using BlockSense.Contracts.Cryptography.Utilities;
using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.FileManagement;
using NBitcoin;
using Nethereum.HdWallet;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class WalletService : IWalletService
    {
        private const string WalletKey = "wallet:active";

        // BIP44 paths
        private const string BtcPath = "m/44'/0'/0'/0/0";
        private const string EthPath = "m/44'/60'/0'/0/0";

        private readonly Aes256GcmEncryptor _aes256GcmEncryptor = new();
        private readonly LevelDbStorage _dbStorage;

        public WalletService(LevelDbStorage dbStorage)
        {
            _dbStorage = dbStorage ?? throw new ArgumentNullException(nameof(dbStorage));
        }

        public static string GenerateMnemonic()
            => new Mnemonic(Wordlist.English, WordCount.Twelve).ToString();

        public async Task<bool> WalletExistsAsync(CancellationToken cancellationToken = default)
            => await _dbStorage.GetAsync<WalletData>(WalletKey, cancellationToken) is not null;

        public Task<WalletData?> LoadWalletAsync(CancellationToken cancellationToken = default)
            => _dbStorage.GetAsync<WalletData>(WalletKey, cancellationToken);

        public Task DeleteWalletAsync(CancellationToken cancellationToken = default)
            => _dbStorage.DeleteAsync(WalletKey, cancellationToken);

        /// <summary>
        /// Encrypts and saves the wallet derived from <paramref name="mnemonic"/> using <paramref name="pin"/>.
        /// Overwrites any existing wallet.
        /// </summary>
        public async Task<WalletData> CreateWalletAsync(
            string mnemonic,
            string pin,
            CancellationToken cancellationToken = default)
        {
            var wallet = Build(mnemonic, pin);
            await _dbStorage.PutAsync(WalletKey, wallet, cancellationToken);
            return wallet;
        }

        /// <summary>
        /// Validates, encrypts, and saves a wallet from an existing mnemonic phrase.
        /// Overwrites any existing wallet.
        /// </summary>
        public async Task<WalletData> ImportWalletAsync(
            string mnemonic,
            string pin,
            CancellationToken cancellationToken = default)
        {
            var parsed = new Mnemonic(mnemonic.Trim(), Wordlist.English);

            if (!parsed.IsValidChecksum)
                throw new ArgumentException("Invalid mnemonic checksum.", nameof(mnemonic));

            var wallet = Build(parsed.ToString(), pin);
            await _dbStorage.PutAsync(WalletKey, wallet, cancellationToken);
            return wallet;
        }

        /// <summary>
        /// Returns true if <paramref name="pin"/> successfully decrypts the seed.
        /// Use this to gate access on app launch.
        /// </summary>
        public bool ValidatePin(WalletData wallet, string pin)
        {
            try
            {
                var key = DeriveKey(pin, wallet.Salt);
                _aes256GcmEncryptor.Decrypt(key, wallet.Iv, wallet.EncryptedSeed);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private WalletData Build(string mnemonicPhrase, string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
                throw new ArgumentNullException(nameof(pin));

            var mnemonic = new Mnemonic(mnemonicPhrase, Wordlist.English);
            var seedBytes = mnemonic.DeriveExtKey().PrivateKey.ToBytes();

            var salt = CryptographyUtilities.GenerateSecureRandomBytes(16);
            var iv = CryptographyUtilities.GenerateSecureRandomBytes(12);
            var key = DeriveKey(pin, salt);

            var encryptedSeed = _aes256GcmEncryptor.Encrypt(key, iv, seedBytes);

            var (btcAddress, btcPubKey) = DeriveBitcoin(seedBytes);
            var (ethAddress, ethPubKey) = DeriveEthereum(seedBytes);

            return new WalletData
            {
                EncryptedSeed = encryptedSeed,
                Iv = iv,
                Salt = salt,
                BtcAddress = btcAddress,
                BtcPublicKey = btcPubKey,
                EthAddress = ethAddress,
                EthPublicKey = ethPubKey,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static (string Address, string PubKeyHex) DeriveBitcoin(byte[] seed)
        {
            var derived = ExtKey.CreateFromSeed(seed).Derive(new KeyPath(BtcPath));

            return (
                derived.PrivateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main).ToString(),
                derived.PrivateKey.PubKey.ToHex()
                );
        }

        private static (string Address, string PubKeyHex) DeriveEthereum(byte[] seed)
        {
            var address = new Wallet(seed).GetAccount(0).Address;
            var derived = ExtKey.CreateFromSeed(seed).Derive(new KeyPath(EthPath));

            return (
                address,
                derived.PrivateKey.PubKey.ToHex()
                );
        }

        private static byte[] DeriveKey(string pin, byte[] salt)
            => System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
                System.Text.Encoding.UTF8.GetBytes(pin),
                salt,
                iterations: 200_000,
                hashAlgorithm: System.Security.Cryptography.HashAlgorithmName.SHA256,
                outputLength: 32);
    }
}
