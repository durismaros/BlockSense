using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Desktop.Utilities.UIComponents;
using System;

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
        await _navigationManager.NavigateToAsync<AuthenticationView>();
    }

    private async void RegisterAsync(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<RegistrationView>();
    }
}