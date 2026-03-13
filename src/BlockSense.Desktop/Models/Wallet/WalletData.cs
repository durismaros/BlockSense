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

        public required DateTime CreatedAt
        {
            get;
            init;
        }
    }
}
