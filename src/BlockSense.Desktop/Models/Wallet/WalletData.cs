using System;

namespace BlockSense.Desktop.Models.Wallet
{
    public sealed record WalletData
    {
        public required byte[] EncryptedSeed
        {
            get;
            init;
        }

        public required byte[] Iv
        {
            get;
            init;
        }

        public required byte[] Salt
        {
            get;
            init;
        }

        public required string BtcAddress
        {
            get;
            init;
        }

        public required string BtcPublicKey
        {
            get;
            init;
        }

        public required string EthAddress
        {
            get;
            init;
        }

        public required string EthPublicKey
        {
            get;
            init;
        }

        public required DateTimeOffset CreatedAt
        {
            get;
            init;
        }
    }
}
