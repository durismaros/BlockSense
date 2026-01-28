using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlockSense.Desktop;

public partial class WelcomeView : UserControl
{
    private readonly NavigationManager _navigationManager;

    public WelcomeView()
    {
        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        InitializeComponent();

        AuthenticateButton.Click += ToAuthenticationViewClick;
        RegisterButton.Click += ToRegistrationViewClick;
    }

    private async void ToAuthenticationViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<AuthenticationView>();
    }

    private async void ToRegistrationViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<RegistrationView>();
    }
}