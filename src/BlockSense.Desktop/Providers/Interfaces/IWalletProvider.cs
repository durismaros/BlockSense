using BlockSense.Desktop.Models.Wallet;
using System;

namespace BlockSense.Desktop.Providers.Interfaces
{
    public interface IWalletProvider
    {
        WalletData? Wallet
        {
            get;
        }

        WalletCreationContext? CreationContext
        {
            get;
        }

        bool HasWallet
        {
            get;
        }

        event Action? OnWalletChanged;

        void SetWallet(WalletData wallet);
        void SetCreationContext(string mnemonic, bool isImport);
        void ClearCreationContext();
        void Clear();
    }
}
