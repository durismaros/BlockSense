using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class WalletProvider : IWalletProvider
    {
        private readonly ILogger<WalletProvider> _logger;
        private Action? _onWalletChanged;

        public WalletData? Wallet
        {
            get;
            private set;
        }

        public WalletCreationContext? CreationContext
        {
            get;
            private set;
        }

        public bool HasWallet => Wallet is not null;

        public event Action? OnWalletChanged
        {
            add
            {
                _onWalletChanged += value; value?.Invoke();
            }

            remove
            {
                _onWalletChanged -= value;
            }
        }

        public WalletProvider(ILogger<WalletProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SetWallet(WalletData wallet)
        {
            Wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));

            _logger.LogInformation("Wallet loaded (BTC: {Btc}, ETH: {Eth})",
                wallet.BtcAddress, wallet.EthAddress);

            _onWalletChanged?.Invoke();
        }

        public void SetCreationContext(string mnemonic, bool isImport)
        {
            CreationContext = new WalletCreationContext
            {
                Mnemonic = mnemonic,
                IsImport = isImport
            };

            _logger.LogDebug("Pending mnemonic set (import: {IsImport})", isImport);
        }

        public void ClearCreationContext()
        {
            CreationContext = null;
        }

        public void Clear()
        {
            Wallet = null;
            CreationContext = null;

            _logger.LogInformation("Wallet provider cleared");
            _onWalletChanged?.Invoke();
        }
    }
}
