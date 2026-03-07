using BlockSense.Desktop.Models.Wallet;

namespace BlockSense.Desktop.Providers.Interfaces
{
    public interface ICurrentWalletProvider
    {
        WalletData? Wallet
        {
            get;
        }

        bool HasWallet
        {
            get;
        }

        void Set(WalletData wallet);
        void Clear();
    }
}
