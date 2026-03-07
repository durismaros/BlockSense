using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Implementations;
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
        private readonly IWalletService _walletService;

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

        public decimal EurValue
        {
            get;
            private set;
        }

        public IReadOnlyList<TransactionDto> Transactions
        {
            get;
            private set;
        }

        public DateTime? LastRefreshed
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

        public EthereumProvider(IEthereumService ethereumService, IWalletService walletService)
        {
            _ethereumService = ethereumService
                ?? throw new ArgumentNullException(nameof(ethereumService));

            _walletService = walletService
                ?? throw new ArgumentNullException(nameof(walletService));

            Address = string.Empty;
            Transactions = new List<TransactionDto>().AsReadOnly();
        }

        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(Address))
            {
                return;
            }

            try
            {
                var balanceTask = _ethereumService.GetBalanceAsync(Address, cancellationToken);
                var rateTask = _walletService.GetRateAsync("BTC", "EUR", cancellationToken);
                var txTask = _ethereumService.GetTransactionsAsync(Address, cancellationToken);

                await Task.WhenAll(balanceTask, txTask, rateTask);

                if (balanceTask.Result is null ||
                    rateTask.Result is null ||
                    txTask.Result is null)
                {
                    return;
                }

                var balance = balanceTask.Result.Balance;
                var rate = rateTask.Result.Rate;
                var transactions = txTask.Result.Transactions
                    .ToList()
                    .AsReadOnly();

                Balance = balance;
                EurValue = balance * rate;
                Transactions = transactions;
                LastRefreshed = DateTime.UtcNow;

                _onChanged?.Invoke();
            }
            catch
            {

            }
        }
    }
}
