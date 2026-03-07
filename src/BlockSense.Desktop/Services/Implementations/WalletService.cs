using BlockSense.Contracts.Cryptography.Encryption;
using BlockSense.Contracts.Cryptography.KeyDerivation;
using BlockSense.Contracts.Cryptography.Utilities;
using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.DTOs.Wallet;
using BlockSense.Desktop.Models.Api;
using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.FileManagement;
using NBitcoin;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Services.Implementations
{
    public sealed class WalletService : IWalletService
    {
        private const string WalletKey = "wallet:active";

        private readonly IApiClient _apiClient;
        private readonly LevelDbStorage _dbStorage;
        private readonly Aes256GcmEncryptor _aes256GcmEncryptor = new();

        public WalletService(IApiClient apiClient, LevelDbStorage dbStorage)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _dbStorage = dbStorage ?? throw new ArgumentNullException(nameof(dbStorage));
        }

        public static string GenerateMnemonic()
            => new Mnemonic(Wordlist.English, WordCount.Twelve).ToString();

        public Task<WalletData?> LoadWalletAsync(CancellationToken cancellationToken = default)
            => _dbStorage.GetAsync<WalletData>(WalletKey, cancellationToken);

        public async Task<WalletData> CreateWalletAsync(
            string mnemonic,
            string pin,
            CancellationToken cancellationToken = default)
        {
            var parsed = new Mnemonic(mnemonic.Trim(), Wordlist.English);

            if (!parsed.IsValidChecksum)
                throw new ArgumentException("Invalid mnemonic checksum.", nameof(mnemonic));

            var wallet = Build(parsed, pin);
            await _dbStorage.PutAsync(WalletKey, wallet, cancellationToken);
            return wallet;
        }

        public async Task<ExchangeRateResponse?> GetRateAsync(string fromAssetSymbol, string toAssetSymbol, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient
                .AddBearerToken()
                .GetAsync<ExchangeRateResponse>($"/api/crypto/exchange-rate/{fromAssetSymbol}/{toAssetSymbol}", cancellationToken);

            if (result.IsSuccess && result is ApiResult<ExchangeRateResponse>.Success success)
            {
                return success.Data;
            }

            return null;
        }

        public async Task<bool> WalletExistsAsync(CancellationToken cancellationToken = default)
            => await _dbStorage.GetAsync<WalletData>(WalletKey, cancellationToken) is not null;

        public Task DeleteWalletAsync(CancellationToken cancellationToken = default)
            => _dbStorage.DeleteAsync(WalletKey, cancellationToken);

        public bool ValidatePin(WalletData wallet, string pin)
        {
            try
            {
                var pinBytes = Encoding.UTF8.GetBytes(pin);
                var key = Pbkdf2Deriver.DeriveBytes(pinBytes, wallet.Salt);
                _aes256GcmEncryptor.Decrypt(key, wallet.Iv, wallet.EncryptedSeed);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private WalletData Build(Mnemonic mnemonic, string pin)
        {
            var seedBytes = mnemonic.DeriveExtKey().PrivateKey.ToBytes();
            var pinBytes = Encoding.UTF8.GetBytes(pin);

            var salt = CryptographyUtilities.GenerateSecureRandomBytes(16);
            var iv = CryptographyUtilities.GenerateSecureRandomBytes(12);
            var key = Pbkdf2Deriver.DeriveBytes(pinBytes, salt);

            var encryptedSeed = _aes256GcmEncryptor.Encrypt(key, iv, seedBytes);

            return new WalletData
            {
                EncryptedSeed = encryptedSeed,
                Iv = iv,
                Salt = salt,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
