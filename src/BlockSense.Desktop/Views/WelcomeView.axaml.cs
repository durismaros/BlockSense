using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Desktop.Utilities.UIComponents;
using System;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class WelcomeView : UserControl
{
    private readonly NavigationManager _navigationManager;

    public WelcomeView(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

        InitializeComponent();
    }

    private async void AuthenticateAsync(object? sender, RoutedEventArgs e)
    {

    }

    private async void RegisterAsync(object? sender, RoutedEventArgs e)
    {

    }
}