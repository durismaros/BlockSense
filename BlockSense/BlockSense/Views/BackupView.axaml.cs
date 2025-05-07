using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlockSense.Client;
using BlockSense.Views;
using Org.BouncyCastle.Crypto.Agreement;

namespace BlockSense;

public partial class BackupView : UserControl
{
    public BackupView()
    {
        InitializeComponent();
    }

    private async void ManualBackupClick(object sender, RoutedEventArgs e)
    {
        await MainWindow.SwitchView(new SecretPhraseView());
    }

    private async void BackupLaterClick(object sender, RoutedEventArgs e)
    {
        await MainWindow.SwitchView(new MainWalletView());
    }
}