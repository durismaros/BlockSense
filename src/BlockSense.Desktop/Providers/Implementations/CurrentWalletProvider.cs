using BlockSense.Contracts.Cryptography.Encryption;
using BlockSense.Contracts.Cryptography.KeyDerivation;
using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class CurrentWalletProvider : ICurrentWalletProvider
    {
        private readonly ILogger<CurrentWalletProvider> _logger;
        private readonly Aes256GcmEncryptor _aes256GcmEncryptor;

        public WalletData? Wallet
        {
            get;
            private set;
        }

        public bool HasWallet
            => Wallet is not null && Wallet.EncryptedSeed is not null;

        public CurrentWalletProvider(ILogger<CurrentWalletProvider> logger)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));

            _aes256GcmEncryptor = new Aes256GcmEncryptor()
                ?? throw new ArgumentNullException(nameof(Aes256GcmEncryptor));
        }

        public void Set(WalletData wallet)
        {
            Wallet = wallet;
        }

        public void Clear()
        {
            Wallet = null;
            _logger.LogInformation("Wallet provider cleared");
        }

        public byte[]? DecryptSeed(string pin)
        {
            if (Wallet is null)
            {
                return null;
            }

            try
            {
                var pinBytes = Encoding.UTF8.GetBytes(pin);
                var key = Pbkdf2Deriver.DeriveBytes(pinBytes, Wallet.Salt);
                return _aes256GcmEncryptor.Decrypt(key, Wallet.Iv, Wallet.EncryptedSeed);
            }
            catch
            {
                return null;
            }
        }
    }
}
