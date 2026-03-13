using BlockSense.Contracts.Cryptography.Encryption;
using BlockSense.Contracts.Cryptography.KeyDerivation;
using BlockSense.Contracts.Cryptography.Utilities;
using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.FileManagement;
using BlockSense.Desktop.Utilities.UIComponents;
using NBitcoin;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    /// <summary>
    /// Implements <see cref="IWalletService"/> to manage the local crypto wallet lifecycle,
    /// including creation, loading, unlocking, and deletion.
    /// The wallet seed is encrypted with the user's PIN and persisted to local storage.
    /// </summary>
    public sealed class WalletService : IWalletService
    {
        private const string WalletStorageKey = "wallet:active";

        private readonly IBitcoinService _bitcoinService;
        private readonly IEthereumService _ethereumService;
        private readonly ICurrentWalletProvider _currentWalletProvider;
        private readonly Aes256GcmEncryptor _aes256GcmEncryptor;
        private readonly LevelDbStorage _dbStorage;
        private readonly NavigationManager _navigationManager;
        private readonly PinEntrySlidingPanel _pinEntrySlidingPanel;

        /// <summary>
        /// Initializes a new instance of <see cref="WalletService"/>.
        /// </summary>
        /// <param name="bitcoinService">The Bitcoin service used to initialize Bitcoin wallet state.</param>
        /// <param name="ethereumService">The Ethereum service used to initialize Ethereum wallet state.</param>
        /// <param name="currentWalletProvider">The provider for accessing and updating the current wallet state.</param>
        /// <param name="navigationManager">The navigation manager used to redirect the user between views.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required dependency is null.</exception>
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

            _navigationManager = navigationManager
                ?? throw new ArgumentNullException(nameof(navigationManager));

            _aes256GcmEncryptor = new Aes256GcmEncryptor();
            _dbStorage = new LevelDbStorage(DirectoryStructure.WalletDirectory);

            _pinEntrySlidingPanel = MainWindow.Instance.PinEntrySlidingPanel
                ?? throw new ArgumentNullException(nameof(PinEntrySlidingPanel));
        }

        /// <summary>
        /// Generates a new 12-word BIP-39 mnemonic phrase.
        /// </summary>
        /// <returns>A space-separated mnemonic phrase as a string.</returns>
        public static string GenerateMnemonic()
            => new Mnemonic(Wordlist.English, WordCount.Twelve).ToString();

        /// <inheritdoc/>
        public Task<WalletData?> LoadWalletAsync(CancellationToken cancellationToken = default)
            => _dbStorage.GetAsync<WalletData>(WalletStorageKey, cancellationToken);

        /// <inheritdoc/>
        public async Task<bool> WalletExistsAsync(CancellationToken cancellationToken = default)
            => await _dbStorage.GetAsync<WalletData>(WalletStorageKey, cancellationToken) is not null;

        /// <inheritdoc/>
        public async Task CreateWalletAsync(Mnemonic mnemonic, string pin, CancellationToken cancellationToken = default)
        {
            var seed = mnemonic.DeriveSeed();

            try
            {
                var wallet = BuildEncryptedWallet(seed, pin);

                await _dbStorage.PutAsync(WalletStorageKey, wallet, cancellationToken);
                _currentWalletProvider.Set(wallet);

                _bitcoinService.Initialize(seed);
                _ethereumService.Initialize(seed);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(seed);
            }
        }

        /// <inheritdoc/>
        public async Task UnlockWalletAsync(CancellationToken cancellationToken = default)
        {
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
                await ExecuteWalletUnlockAsync(pin, cancellationToken));
        }

        /// <inheritdoc/>
        public async Task DeleteWalletAsync(CancellationToken cancellationToken = default)
        {
            _currentWalletProvider.Clear();
            await _dbStorage.DeleteAsync(WalletStorageKey, cancellationToken);
        }

        private async Task ExecuteWalletUnlockAsync(string pin, CancellationToken cancellationToken)
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
        }

        private WalletData BuildEncryptedWallet(byte[] seed, string pin)
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