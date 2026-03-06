using BlockSense.Desktop.Models.Wallet;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    public interface IWalletService
    {
        Task<bool> WalletExistsAsync(CancellationToken cancellationToken = default);
        Task<WalletData?> LoadWalletAsync(CancellationToken cancellationToken = default);
        Task<WalletData> CreateWalletAsync(string mnemonic, string pin, CancellationToken cancellationToken = default);
        Task<WalletData> ImportWalletAsync(string mnemonic, string pin, CancellationToken cancellationToken = default);
        Task DeleteWalletAsync(CancellationToken cancellationToken = default);
        bool ValidatePin(WalletData wallet, string pin);
    }
}
