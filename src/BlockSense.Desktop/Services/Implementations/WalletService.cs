using BlockSense.Contracts.Cryptography.Encryption;
using BlockSense.Contracts.Cryptography.KeyDerivation;
using BlockSense.Contracts.Cryptography.Utilities;
using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Providers.Implementations;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.FileManagement;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class WalletService : IWalletService
    {
        private const string WalletKey = "wallet:active";

        private readonly IBitcoinService _bitcoinService;
        private readonly IEthereumService _ethereumService;
        private readonly ICurrentWalletProvider _currentWalletProvider;
        private readonly Aes256GcmEncryptor _aes256GcmEncryptor;
        private readonly LevelDbStorage _dbStorage;
        private readonly NavigationManager _navigationManager;
        private readonly PinEntrySlidingPanel _pinEntrySlidingPanel;

        public WalletService(
            IBitcoinService bitcoinService,
            IEthereumService ethereumService,
            ICurrentWalletProvider currentWalletProvider,
            NavigationManager navigationManager)
        {
            _bitcoinService = bitcoinService
                ?? throw new ArgumentNullException(nameof(bitcoinService));

            _ethereumService = ethereumService
                ?? throw new ArgumentNullException(nameof(ethereumService));

            _currentWalletProvider = currentWalletProvider
                ?? throw new ArgumentNullException(nameof(currentWalletProvider));

            _aes256GcmEncryptor = new Aes256GcmEncryptor()
                ?? throw new ArgumentNullException(nameof(Aes256GcmEncryptor));

            _dbStorage = new LevelDbStorage(DirectoryStructure.WalletDirectory)
                ?? throw new ArgumentNullException(nameof(LevelDbStorage));

            _navigationManager = navigationManager
                ?? throw new ArgumentNullException(nameof(navigationManager));

            _pinEntrySlidingPanel = MainWindow.Instance.PinEntrySlidingPanel
                ?? throw new ArgumentNullException(nameof(PinEntrySlidingPanel));
        }

        public static string GenerateMnemonic()
            => new Mnemonic(Wordlist.English, WordCount.Twelve).ToString();

        public Task<WalletData?> LoadWalletAsync(CancellationToken cancellationToken = default)
            => _dbStorage.GetAsync<WalletData>(WalletKey, cancellationToken);

        public async Task<bool> WalletExistsAsync(CancellationToken cancellationToken = default)
            => await _dbStorage.GetAsync<WalletData>(WalletKey, cancellationToken) is not null;

        public async Task CreateWalletAsync(Mnemonic mnemonic, string pin, CancellationToken cancellationToken = default)
        {
            var seed = mnemonic.DeriveExtKey().PrivateKey.ToBytes();
            var wallet = Build(seed, pin);

            try
            {
                await _dbStorage.PutAsync(WalletKey, wallet, cancellationToken);
                _currentWalletProvider.Set(wallet);

                _bitcoinService.Initialize(seed);
                _ethereumService.Initialize(seed);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(seed);
            }
        }

        public async Task UnlockWalletAsync(CancellationToken cancellationToken = default)
        {
            // Load wallet into the provider cache if it isn't already there.
            if (!_currentWalletProvider.HasWallet)
            {
                var stored = await LoadWalletAsync(cancellationToken);

                if (stored is null)
                {
                    return;
                }

                _currentWalletProvider.Set(stored);
            }

            _pinEntrySlidingPanel.ShowPanel(async pin =>
            {
                var seed = _currentWalletProvider.DecryptSeed(pin);

                if (seed is null)
                {
                    await _pinEntrySlidingPanel.ShowErrorState();
                    return;
                }

                try
                {
                    _bitcoinService.Initialize(seed);
                    _ethereumService.Initialize(seed);

                    _pinEntrySlidingPanel.HidePanel();
                    await _navigationManager.NavigateToAsync<CryptoWalletView>();
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(seed);
                }
            });
        }

        public async Task DeleteWalletAsync(CancellationToken cancellationToken = default)
        {
            _currentWalletProvider.Clear();
            await _dbStorage.DeleteAsync(WalletKey, cancellationToken);
        }

        private WalletData Build(byte[] seed, string pin)
        {
            var pinBytes = Encoding.UTF8.GetBytes(pin);

            var salt = CryptographyUtilities.GenerateSecureRandomBytes(16);
            var iv = CryptographyUtilities.GenerateSecureRandomBytes(12);
            var key = Pbkdf2Deriver.DeriveBytes(pinBytes, salt);

            var encryptedSeed = _aes256GcmEncryptor.Encrypt(key, iv, seed);

            return new WalletData
            {
                EncryptedSeed = encryptedSeed,
                Iv = iv,
                Salt = salt,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
