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

        private readonly IApiClient _apiClient;
        private readonly ICurrentWalletProvider _currentWalletProvider;
        private readonly IBitcoinProvider _bitcoinProvider;
        private readonly IEthereumProvider _ethereumProvider;
        private readonly NavigationManager _navigationManager;
        private readonly LevelDbStorage _dbStorage;
        private readonly Aes256GcmEncryptor _aes256GcmEncryptor = new();
        private readonly PinEntrySlidingPanel _pinEntrySlidingPanel;

        public WalletService(
            IApiClient apiClient,
            ICurrentWalletProvider currentWalletProvider,
            IBitcoinProvider bitcoinProvider,
            IEthereumProvider ethereumProvider,
            NavigationManager navigationManager)
        {
            _apiClient = apiClient
                ?? throw new ArgumentNullException(nameof(apiClient));

            _currentWalletProvider = currentWalletProvider
                ?? throw new ArgumentNullException(nameof(currentWalletProvider));

            _bitcoinProvider = bitcoinProvider
                ?? throw new ArgumentNullException(nameof(bitcoinProvider));

            _ethereumProvider = ethereumProvider
                ?? throw new ArgumentNullException(nameof(ethereumProvider));

            _navigationManager = navigationManager
                ?? throw new ArgumentNullException(nameof(navigationManager));

            _dbStorage = new LevelDbStorage(DirectoryStructure.WalletDirectory)
                ?? throw new ArgumentNullException(nameof(LevelDbStorage));

            _pinEntrySlidingPanel = MainWindow.Instance.PinEntrySlidingPanel
                ?? throw new ArgumentNullException(nameof(PinEntrySlidingPanel));
        }

        public static string GenerateMnemonic()
            => new Mnemonic(Wordlist.English, WordCount.Twelve).ToString();

        public Task<WalletData?> LoadWalletAsync(CancellationToken cancellationToken = default)
            => _dbStorage.GetAsync<WalletData>(WalletKey, cancellationToken);

        public async Task<WalletData> CreateWalletAsync(Mnemonic mnemonic, string pin, CancellationToken cancellationToken = default)
        {
            var wallet = Build(mnemonic, pin);
            await _dbStorage.PutAsync(WalletKey, wallet, cancellationToken);


            return wallet;
        }

        public async Task<bool> WalletExistsAsync(CancellationToken cancellationToken = default)
            => await _dbStorage.GetAsync<WalletData>(WalletKey, cancellationToken) is not null;

        public async Task DeleteWalletAsync(CancellationToken cancellationToken = default)
        {
            _currentWalletProvider.Clear();
            await _dbStorage.DeleteAsync(WalletKey, cancellationToken);
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
                    _bitcoinProvider.Initialize(seed);
                    _ethereumProvider.Initialize(seed);

                    _pinEntrySlidingPanel.HidePanel();
                    await _navigationManager.NavigateToAsync<CryptoWalletView>();
                }
                finally
                {
                    CryptographicOperations.ZeroMemory(seed);
                }
            });
        }

        private WalletData Build(Mnemonic mnemonic, string pin)
        {
            var seedBytes = mnemonic.DeriveExtKey().PrivateKey.ToBytes();
            var pinBytes = Encoding.UTF8.GetBytes(pin);

            var salt = CryptographyUtilities.GenerateSecureRandomBytes(16);
            var iv = CryptographyUtilities.GenerateSecureRandomBytes(12);
            var key = Pbkdf2Deriver.DeriveBytes(pinBytes, salt);

            var encryptedSeed = _aes256GcmEncryptor.Encrypt(key, iv, seedBytes);

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
