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
    public sealed class EthereumProvider : IEthereumProvider
    {
        private readonly IEthereumService _ethereumService;

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

        public EthereumProvider(IEthereumService ethereumService)
        {
            _ethereumService = ethereumService
                ?? throw new ArgumentNullException(nameof(ethereumService));

            Address = string.Empty;
            Transactions = new List<TransactionDto>().AsReadOnly();
        }

        public void Initialize(byte[] seed)
        {
            Address = _ethereumService.DeriveAddress(seed);
        }

        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(Address))
            {
                return;
            }

            try
            {
                var balance = await _ethereumService.GetBalanceAsync(Address, cancellationToken);
                var rate = await _ethereumService.GetExchangeRateAsync("EUR", cancellationToken);
                var txs = await _ethereumService.GetTransactionsAsync(Address, cancellationToken);

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
