using BlockSense.Desktop.Models.Wallet;
using NBitcoin;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface IWalletService
    {
        Task<WalletData?> LoadWalletAsync(CancellationToken cancellationToken = default);
        Task<bool> WalletExistsAsync(CancellationToken cancellationToken = default);
        Task CreateWalletAsync(Mnemonic mnemonic, string pin, CancellationToken cancellationToken = default);
        Task UnlockWalletAsync(CancellationToken cancellationToken = default);
        Task DeleteWalletAsync(CancellationToken cancellationToken = default);

    }
}
