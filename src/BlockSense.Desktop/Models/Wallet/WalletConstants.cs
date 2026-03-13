using NBitcoin;

namespace BlockSense.Desktop.Models.Wallet
{
    public static class BitcoinFees
    {
        public const decimal Default = 0.00002260m;
    }

    public static class EthereumFees
    {
        public const long DefaultGasLimit = 21_000;

        public const decimal DefaultGasPriceGwei = 20m;
    }

    public static class BitcoinChain
    {
        public static readonly Network CurrentNetwork = Network.TestNet;

        // Mainnet = "m/44'/0'/0'/0/0"
        public static readonly KeyPath DerivationPath = new("m/44'/1'/0'/0");
    }

    public static class EthereumChain
    {
        // Sepolia = 11155111
        // Mainnet = 1
        public const int CurrentNetwork = 11155111;

        public static readonly KeyPath DerivationPath = new("m/44'/60'/0'/0");
    }
}
