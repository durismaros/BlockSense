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

        AuthenticateButton.Click += ToAuthViewClick;
        RegisterButton.Click += ToRegisterViewClick;
    }

    private async void ToAuthViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<AuthenticationView>();
    }

    private async void ToRegisterViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<RegistrationView>();
    }
}