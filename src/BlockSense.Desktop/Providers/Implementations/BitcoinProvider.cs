using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop.Providers.Implementations
{
    public sealed class BitcoinProvider : IBitcoinProvider
    {
        private readonly IBitcoinService _bitcoinService;

        private Action? _onChanged;

        public string Address
        {
            get;
            private set;
        }

        public decimal Balance
        {
            get;
            private set;
        }

        public decimal ExchangeRate
        {
            get;
            private set;
        }

        public IReadOnlyList<TransactionDto> Transactions
        {
            get;
            private set; 
        }

        public event Action? OnChanged
        {
            add
            {
                _onChanged += value;
            }
            remove
            {
                _onChanged -= value;
            }
        }

        public BitcoinProvider(IBitcoinService bitcoinService)
        {
            _bitcoinService = bitcoinService
                ?? throw new ArgumentNullException(nameof(bitcoinService));

            Address = string.Empty;
            Transactions = new List<TransactionDto>().AsReadOnly();
        }

        public void Initialize(byte[] seed)
        {
            Address = _bitcoinService.DeriveAddress(seed);
        }

        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(Address))
            {
                return;
            }

            try
            {
                var balance = await _bitcoinService.GetBalanceAsync(Address, cancellationToken);
                var rate = await _bitcoinService.GetExchangeRateAsync("EUR", cancellationToken);
                var txs = await _bitcoinService.GetTransactionsAsync(Address, cancellationToken);

                if (balance is null || rate is null || txs is null)
                {
                    return;
                }

                Balance = balance.Balance;
                ExchangeRate = rate.Rate;
                Transactions = txs.Transactions.ToList().AsReadOnly();

                _onChanged?.Invoke();
            }
            catch
            {

            }
        }
    }
}
