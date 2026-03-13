using BlockSense.Desktop.Models.Wallet;
using NBitcoin;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Interfaces
{
    /// <summary>
    /// Defines methods for managing the local crypto wallet lifecycle,
    /// including creation, loading, unlocking, and deletion.
    /// </summary>
    public interface IWalletService
    {
        /// <summary>
        /// Loads the stored wallet data from persistent storage.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="WalletData"/> instance if a wallet exists in storage;
        /// otherwise, <c>null</c>.
        /// </returns>
        Task<WalletData?> LoadWalletAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines whether a wallet has been created and stored on this device.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns><c>true</c> if a wallet exists in storage; otherwise, <c>false</c>.</returns>
        Task<bool> WalletExistsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new wallet by deriving a seed from the provided mnemonic,
        /// encrypting it with the given PIN, and persisting it to storage.
        /// Also initializes the Bitcoin and Ethereum services with the derived seed.
        /// </summary>
        /// <param name="mnemonic">The BIP-39 mnemonic used to derive the wallet seed.</param>
        /// <param name="pin">The PIN used to encrypt the wallet seed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task CreateWalletAsync(Mnemonic mnemonic, string pin, CancellationToken cancellationToken = default);

        /// <summary>
        /// Prompts the user for their PIN to decrypt and unlock the existing wallet.
        /// On success, initializes the Bitcoin and Ethereum services and navigates to the wallet view.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task UnlockWalletAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the stored wallet from persistent storage and clears the current wallet provider.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        Task DeleteWalletAsync(CancellationToken cancellationToken = default);
    }
}