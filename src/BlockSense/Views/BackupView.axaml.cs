using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlockSense.Client;
using BlockSense.Services;
using BlockSense.Utilities.UI;
using BlockSense.ViewModels;
using BlockSense.Views;
using Org.BouncyCastle.Crypto.Agreement;

namespace BlockSense;

public partial class BackupView : UserControl
{
    private readonly NavigationService _navigationService;
    public BackupView(NavigationService navigationService)
    {
        _navigationService = navigationService;
        InitializeComponent();
    }

    private async void ManualBackupClick(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateTo<SecretPhraseViewModel>();
    }

    private async void BackupLaterClick(object sender, RoutedEventArgs e)
    {
        _navigationService.NavigateTo<MainWalletViewModel>();
    }
}