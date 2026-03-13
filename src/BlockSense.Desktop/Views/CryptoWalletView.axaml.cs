using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using BlockSense.Contracts.DTOs.Transaction;
using BlockSense.Contracts.Enums;
using BlockSense.Desktop.Models.Wallet;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

/// <summary>
/// View that displays Bitcoin and Ethereum wallet cards, transaction history,
/// and a send overlay for broadcasting transactions.
/// </summary>
public partial class CryptoWalletView : UserControl
{
    private enum ActiveTab { All, Bitcoin, Ethereum }
    private enum SendCurrency { Bitcoin, Ethereum }

    private readonly IWalletService _walletService;
    private readonly IBitcoinService _bitcoinService;
    private readonly IEthereumService _ethereumService;
    private readonly IBitcoinProvider _bitcoinProvider;
    private readonly IEthereumProvider _ethereumProvider;
    private readonly NavigationManager _navigationManager;
    private readonly PinEntrySlidingPanel _pinEntrySlidingPanel;
    private readonly ILogger<CryptoWalletView> _logger;

    private ActiveTab _activeTab = ActiveTab.All;
    private SendCurrency _sendCurrency = SendCurrency.Bitcoin;
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// Initialises a new instance of <see cref="CryptoWalletView"/>.
    /// </summary>
    public CryptoWalletView()
    {
        _walletService = App.ServiceProvider.GetRequiredService<IWalletService>();
        _bitcoinService = App.ServiceProvider.GetRequiredService<IBitcoinService>();
        _ethereumService = App.ServiceProvider.GetRequiredService<IEthereumService>();
        _bitcoinProvider = App.ServiceProvider.GetRequiredService<IBitcoinProvider>();
        _ethereumProvider = App.ServiceProvider.GetRequiredService<IEthereumProvider>();
        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>();
        _pinEntrySlidingPanel = MainWindow.Instance.PinEntrySlidingPanel;
        _logger = App.ServiceProvider.GetRequiredService<ILogger<CryptoWalletView>>();

        InitializeComponent();
        _ = RefreshAllAsync();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _cancellationTokenSource = new CancellationTokenSource();

        RenderAddresses();
        RenderTransactions();

        _bitcoinProvider.OnChanged += OnBitcoinProviderChanged;
        _ethereumProvider.OnChanged += OnEthereumProviderChanged;

        BackButton.Click += OnBackButtonClicked;
        RemoveWalletButton.Click += OnRemoveWalletButtonClicked;
        ConfirmDeleteButton.Click += OnConfirmDeleteButtonClicked;
        SendButton.Click += OnSendButtonClicked;
        CloseSendOverlayButton.Click += OnCloseSendOverlayButtonClicked;
        ConfirmSendButton.Click += OnConfirmSendButtonClicked;
        SendBtcToggle.Click += OnSendBtcToggleClicked;
        SendEthToggle.Click += OnSendEthToggleClicked;
        CopyBtcAddressButton.Click += OnCopyBtcAddressButtonClicked;
        CopyEthAddressButton.Click += OnCopyEthAddressButtonClicked;
        RefreshBtcButton.Click += OnRefreshBtcButtonClicked;
        RefreshEthButton.Click += OnRefreshEthButtonClicked;
        AllTabButton.Click += OnAllTabButtonClicked;
        BtcTabButton.Click += OnBtcTabButtonClicked;
        EthTabButton.Click += OnEthTabButtonClicked;
        SendAmountInput.TextChanged += OnSendAmountInputTextChanged;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _bitcoinProvider.OnChanged -= OnBitcoinProviderChanged;
        _ethereumProvider.OnChanged -= OnEthereumProviderChanged;

        BackButton.Click -= OnBackButtonClicked;
        RemoveWalletButton.Click -= OnRemoveWalletButtonClicked;
        ConfirmDeleteButton.Click -= OnConfirmDeleteButtonClicked;
        SendButton.Click -= OnSendButtonClicked;
        CloseSendOverlayButton.Click -= OnCloseSendOverlayButtonClicked;
        ConfirmSendButton.Click -= OnConfirmSendButtonClicked;
        SendBtcToggle.Click -= OnSendBtcToggleClicked;
        SendEthToggle.Click -= OnSendEthToggleClicked;
        CopyBtcAddressButton.Click -= OnCopyBtcAddressButtonClicked;
        CopyEthAddressButton.Click -= OnCopyEthAddressButtonClicked;
        RefreshBtcButton.Click -= OnRefreshBtcButtonClicked;
        RefreshEthButton.Click -= OnRefreshEthButtonClicked;
        AllTabButton.Click -= OnAllTabButtonClicked;
        BtcTabButton.Click -= OnBtcTabButtonClicked;
        EthTabButton.Click -= OnEthTabButtonClicked;
        SendAmountInput.TextChanged -= OnSendAmountInputTextChanged;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    private async void OnBackButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await _navigationManager.NavigateToAsync<HomeView>();

    private async void OnRemoveWalletButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await ShowConfirmDeleteAsync();

    private async void OnConfirmDeleteButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await DeleteWalletAsync();

    private async void OnSendButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await OpenSendOverlayAsync();

    private async void OnCloseSendOverlayButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await CloseSendOverlayAsync();

    private async void OnConfirmSendButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await ExecuteSendAsync();

    private void OnSendBtcToggleClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => SetSendCurrency(SendCurrency.Bitcoin);

    private void OnSendEthToggleClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => SetSendCurrency(SendCurrency.Ethereum);

    private void OnCopyBtcAddressButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => CopyToClipboard(BtcAddressText.Text);

    private void OnCopyEthAddressButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => CopyToClipboard(EthAddressText.Text);

    private async void OnRefreshBtcButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RefreshBtcButton.IsEnabled = false;
        await _bitcoinService.RefreshAsync(_cancellationTokenSource?.Token ?? default);
        RefreshBtcButton.IsEnabled = true;
    }

    private async void OnRefreshEthButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RefreshEthButton.IsEnabled = false;
        await _ethereumService.RefreshAsync(_cancellationTokenSource?.Token ?? default);
        RefreshEthButton.IsEnabled = true;
    }

    private void OnAllTabButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => SetActiveTab(ActiveTab.All);
    private void OnBtcTabButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => SetActiveTab(ActiveTab.Bitcoin);
    private void OnEthTabButtonClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => SetActiveTab(ActiveTab.Ethereum);

    private void OnSendAmountInputTextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e)
        => UpdateConversionSummary();

    private void OnBitcoinProviderChanged() => Dispatcher.UIThread.Post(UpdateBitcoinDisplay);
    private void OnEthereumProviderChanged() => Dispatcher.UIThread.Post(UpdateEthereumDisplay);

    private async Task OpenSendOverlayAsync()
    {
        SendAddressInput.Text = string.Empty;
        SendAmountInput.Text = string.Empty;
        SetSendCurrency(SendCurrency.Bitcoin);
        UpdateConversionSummary();

        SendOverlay.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(SendOverlay);
    }

    private async Task CloseSendOverlayAsync()
    {
        await Animations.FadeOutAnimation.RunAsync(SendOverlay);
        SendOverlay.IsVisible = false;
    }

    private void SetSendCurrency(SendCurrency currency)
    {
        _sendCurrency = currency;

        SendBtcToggle.Classes.Set("Active", currency == SendCurrency.Bitcoin);
        SendEthToggle.Classes.Set("Active", currency == SendCurrency.Ethereum);

        SendBtcToggleText.Foreground = new SolidColorBrush(Color.Parse(
            currency == SendCurrency.Bitcoin ? "#E8DDD0" : "#4A3F30"));
        SendEthToggleText.Foreground = new SolidColorBrush(Color.Parse(
            currency == SendCurrency.Ethereum ? "#E8DDD0" : "#4A3F30"));

        UpdateConversionSummary();
    }

    private void UpdateConversionSummary()
    {
        if (SendAmountInput is null)
            return;

        bool isBitcoin = _sendCurrency == SendCurrency.Bitcoin;
        var ticker = isBitcoin ? "BTC" : "ETH";
        var rate = isBitcoin ? _bitcoinProvider.ExchangeRate : _ethereumProvider.ExchangeRate;
        var fee = ResolveNetworkFee();

        _ = decimal.TryParse(
            SendAmountInput.Text,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out var amount);

        var total = amount + fee;

        if (SendConversionText is not null) SendConversionText.Text = $"€{amount * rate:N2}";
        if (SendConversionLabel is not null) SendConversionLabel.Text = $"≈ EUR value for {amount:F6} {ticker}";
        if (SendExchangeRateText is not null) SendExchangeRateText.Text = $"1 {ticker} = €{rate:N2}";
        if (SendFeeText is not null) SendFeeText.Text = $"{fee:F8} {ticker}";
        if (SendFeeEurText is not null) SendFeeEurText.Text = $"≈ €{fee * rate:N2}";
        if (SendTotalText is not null) SendTotalText.Text = $"{total:F8} {ticker}";
        if (SendTotalEurText is not null) SendTotalEurText.Text = $"≈ €{total * rate:N2}";
    }

    private decimal ResolveNetworkFee() => _sendCurrency switch
    {
        SendCurrency.Bitcoin => BitcoinFees.Default,
        SendCurrency.Ethereum => EthereumFees.DefaultGasLimit * EthereumFees.DefaultGasPriceGwei / 1_000_000_000m,
        _ => 0m
    };

    private async Task ExecuteSendAsync()
    {
        var address = SendAddressInput.Text?.Trim() ?? string.Empty;
        var amountStr = SendAmountInput.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(address))
            return;

        if (!decimal.TryParse(amountStr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var amount) || amount <= 0)
            return;

        var token = _cancellationTokenSource?.Token ?? default;

        try
        {
            if (_sendCurrency == SendCurrency.Bitcoin)
                await _bitcoinService.SignAndBroadcastAsync(address, amount, token);
            else
                await _ethereumService.SignAndBroadcastAsync(address, amount, token);

            await CloseSendOverlayAsync();

            _logger.LogInformation("Transaction sent: {Amount} {Currency} → {Address}.",
                amount, _sendCurrency, address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast transaction.");
        }
    }

    private void UpdateBitcoinDisplay()
    {
        BtcBalanceText.Text = $"{_bitcoinProvider.Balance:F8} BTC";
        BtcEurValueText.Text = $"≈ €{_bitcoinProvider.Balance * _bitcoinProvider.ExchangeRate:N2} EUR";
        RenderTransactions();
        UpdateConversionSummary();
    }

    private void UpdateEthereumDisplay()
    {
        EthBalanceText.Text = $"{_ethereumProvider.Balance:F8} ETH";
        EthEurValueText.Text = $"≈ €{_ethereumProvider.Balance * _ethereumProvider.ExchangeRate:N2} EUR";
        RenderTransactions();
        UpdateConversionSummary();
    }

    private void RenderAddresses()
    {
        BtcAddressText.Text = _bitcoinProvider.Address;
        EthAddressText.Text = _ethereumProvider.Address;
    }

    private void SetActiveTab(ActiveTab tab)
    {
        _activeTab = tab;

        AllTabButton.Classes.Set("Active", tab == ActiveTab.All);
        BtcTabButton.Classes.Set("Active", tab == ActiveTab.Bitcoin);
        EthTabButton.Classes.Set("Active", tab == ActiveTab.Ethereum);

        RenderTransactions();
    }

    private void RenderTransactions()
    {
        var transactions = GetFilteredTransactions();
        TransactionsPanel.Children.Clear();

        if (!transactions.Any())
        {
            TransactionsPanel.Children.Add(BuildEmptyTransactionsState());
            return;
        }

        for (int i = 0; i < transactions.Count; i++)
        {
            if (i > 0)
                TransactionsPanel.Children.Add(BuildTransactionDivider());

            TransactionsPanel.Children.Add(BuildTransactionRow(transactions[i]));
        }
    }

    private List<TransactionDto> GetFilteredTransactions()
    {
        var bitcoin = _bitcoinProvider.Transactions
            .Select(tx => tx with
            {
                Amount = IsOutgoing(tx.FromAddress, _bitcoinProvider.Address) ? -tx.Amount : tx.Amount
            });

        var ethereum = _ethereumProvider.Transactions
            .Select(tx => tx with
            {
                Amount = IsOutgoing(tx.FromAddress, _ethereumProvider.Address) ? -tx.Amount : tx.Amount
            });

        return _activeTab switch
        {
            ActiveTab.Bitcoin => bitcoin.ToList(),
            ActiveTab.Ethereum => ethereum.ToList(),
            _ => bitcoin.Concat(ethereum).OrderByDescending(t => t.Timestamp).ToList()
        };
    }

    private static bool IsOutgoing(string fromAddress, string walletAddress) =>
        fromAddress.Equals(walletAddress, StringComparison.OrdinalIgnoreCase);

    private async Task ShowConfirmDeleteAsync()
    {
        ConfirmDeleteButton.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(ConfirmDeleteButton);

        await Task.Delay(3000);

        await Animations.FadeOutAnimation.RunAsync(ConfirmDeleteButton);
        ConfirmDeleteButton.IsVisible = false;
    }

    private async Task DeleteWalletAsync()
    {
        _logger.LogWarning("Deleting current wallet.");
        await _walletService.DeleteWalletAsync();
        await _navigationManager.NavigateToAsync<HomeView>();
    }

    private async Task RefreshAllAsync()
    {
        var token = _cancellationTokenSource?.Token ?? default;
        await Task.WhenAll(
            _bitcoinService.RefreshAsync(token),
            _ethereumService.RefreshAsync(token));
    }

    private void CopyToClipboard(string? text)
    {
        if (!string.IsNullOrWhiteSpace(text))
            TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(text);
    }

    private static Control BuildEmptyTransactionsState() => new Border
    {
        Padding = new Thickness(40, 48),
        Child = new StackPanel
        {
            Spacing = 10,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children =
            {
                new Border
                {
                    Width = 44, Height = 44,
                    CornerRadius = new CornerRadius(22),
                    Background = new SolidColorBrush(Color.Parse("#C4BBAA")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Child = new Viewbox
                    {
                        Width = 20, Height = 20,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Child = new Path
                        {
                            Data = Avalonia.Media.Geometry.Parse(
                                "M13 17H11V15H13V17ZM13 13H11V7H13V13ZM12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2ZM12 20C7.59 20 4 16.41 4 12C4 7.59 7.59 4 12 4C16.41 4 20 7.59 20 12C20 16.41 16.41 20 12 20Z"),
                            Fill = new SolidColorBrush(Color.Parse("#8A7E6E")),
                            Stretch = Stretch.Uniform
                        }
                    }
                },
                BuildText("No transactions yet", "#4A4540", 14, FontWeight.SemiBold,
                    horizontalAlignment: HorizontalAlignment.Center),
                BuildText("Your transaction history will appear here", "#8A7E6E", 12,
                    horizontalAlignment: HorizontalAlignment.Center)
            }
        }
    };

    private static Border BuildTransactionDivider() => new()
    {
        Height = 1,
        Background = new SolidColorBrush(Color.Parse("#C0B8A8")),
        Margin = new Thickness(20, 0)
    };

    private static Border BuildTransactionRow(TransactionDto tx)
    {
        bool isIncoming = tx.Amount >= 0;
        bool isFailed = tx.Status == TransactionStatus.Failed;

        var accentHex = isFailed ? "#9A8F7E" : isIncoming ? "#3D9970" : "#C0392B";

        var (statusHex, statusText) = tx.Status switch
        {
            TransactionStatus.Confirmed => ("#3D9970", "Confirmed"),
            TransactionStatus.Pending => ("#C8A837", "Pending"),
            TransactionStatus.Failed => ("#C0392B", "Failed"),
            _ => ("#9A8F7E", "Unknown")
        };

        var counterparty = isIncoming ? tx.FromAddress : tx.ToAddress;
        var directionLabel = isIncoming ? "From" : "To";
        var amountText = $"{(isIncoming ? "+" : "-")}{Math.Abs(tx.Amount):F6} {tx.Currency}";

        var statusPill = new Border
        {
            Background = new SolidColorBrush(Color.Parse(statusHex + "22")),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(8, 4),
            Child = BuildText(statusText, statusHex, 10, FontWeight.SemiBold,
                horizontalAlignment: HorizontalAlignment.Center,
                verticalAlignment: VerticalAlignment.Center)
        };

        var directionBadge = new Border
        {
            Background = new SolidColorBrush(Color.Parse(accentHex + "22")),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(6, 2),
            Child = BuildText(directionLabel, accentHex, 10, FontWeight.SemiBold)
        };

        var leftColumn = new StackPanel
        {
            Spacing = 5,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Children = { directionBadge, BuildText(tx.TxHash, "#5A4E42", 11, fontFamily: "Consolas") }
                },
                BuildText(counterparty ?? "—", "#7A6E62", 11, fontFamily: "Consolas"),
                BuildText(tx.Timestamp.ToString("dd MMM yyyy · HH:mm"), "#9A8F7E", 10)
            }
        };

        var rightColumn = new StackPanel
        {
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                BuildText(amountText, accentHex, 13, FontWeight.SemiBold,
                    horizontalAlignment: HorizontalAlignment.Right),
                statusPill
            }
        };

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("* Auto") };
        Grid.SetColumn(leftColumn, 0);
        Grid.SetColumn(rightColumn, 1);
        grid.Children.Add(leftColumn);
        grid.Children.Add(rightColumn);

        return new Border { Classes = { "TxRow" }, Child = grid };
    }

    private static TextBlock BuildText(
        string text,
        string colorHex,
        double fontSize,
        FontWeight fontWeight = FontWeight.Normal,
        string? fontFamily = null,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment verticalAlignment = VerticalAlignment.Center) => new()
        {
            Text = text,
            Foreground = new SolidColorBrush(Color.Parse(colorHex)),
            FontSize = fontSize,
            FontWeight = fontWeight,
            FontFamily = fontFamily is not null ? new FontFamily(fontFamily) : FontFamily.Default,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
}
