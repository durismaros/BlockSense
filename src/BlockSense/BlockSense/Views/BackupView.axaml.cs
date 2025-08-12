using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlockSense.Client;
using BlockSense.Utilities;
using BlockSense.Views;
using Org.BouncyCastle.Crypto.Agreement;

namespace BlockSense;

public partial class BackupView : UserControl
{
    private readonly IViewSwitcher _viewSwitcher;
    public BackupView(IViewSwitcher viewSwitcher)
    {
        _viewSwitcher = viewSwitcher;
        InitializeComponent();
    }

    private async void ManualBackupClick(object sender, RoutedEventArgs e)
    {
        await _viewSwitcher.NavigateToAsync<SecretPhraseView>();
    }

    private async void BackupLaterClick(object sender, RoutedEventArgs e)
    {
        await _viewSwitcher.NavigateToAsync<MainWalletView>();
    }
}