using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using System;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class CurrentWalletProvider : ICurrentWalletProvider
    {
        private readonly ILogger<CurrentWalletProvider> _logger;

        public WalletData? Wallet
        {
            get;
            private set;
        }

        public bool HasWallet => Wallet is not null;

        public CurrentWalletProvider(ILogger<CurrentWalletProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    }
}
