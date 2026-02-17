using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using BlockSense.Desktop.Utilities.Validation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace BlockSense.Desktop;

public partial class AuthenticationView : UserControl
{
    private readonly IAuthService _authService;
    private readonly NavigationManager _navigationManager;

    private CancellationTokenSource? _cancellationTokenSource;

    public AuthenticationView()
    {
        _authService = App.ServiceProvider.GetRequiredService<IAuthService>()
            ?? throw new ArgumentNullException(nameof(IAuthService));

        _navigationManager = App.ServiceProvider.GetRequiredService<NavigationManager>()
            ?? throw new ArgumentNullException(nameof(NavigationManager));

        InitializeComponent();

        this.AttachedToVisualTree += OnAttachedToVisualTree;
        this.DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    private async void ToWelcomeViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WelcomeView>();
    }

    private async void AuthenticateClick(object? sender = default, RoutedEventArgs? e = default)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        var request = new AuthRequest
        {
            Login = LoginTextBox.Text?.Trim() ?? string.Empty,
            Password = PasswordTextBox.Text ?? string.Empty,
        };

        if (!DataAnnotationsValidator.TryValidate(request, out var validationError))
        {
            MainWindow.Instance.ShowNotification("Authentication", validationError);
            return;
        }

        await _authService.AuthenticateAsync(request, cancellationToken);
    }

    private async void RevealPasswordClick(object? sender, RoutedEventArgs e)
    {
        PasswordTextBox.PasswordChar = EyeCrossLine.IsVisible ? '●' : '\0';

        if (EyeCrossLine.IsVisible)
        {
            await Animations.FadeOutAnimation.RunAsync(EyeCrossLine);
            EyeCrossLine.IsVisible = false;
        }
        else
        {
            EyeCrossLine.IsVisible = true;
            await Animations.FadeInAnimation.RunAsync(EyeCrossLine);
        }
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _cancellationTokenSource?.Cancel();

        HomeButton.Click += ToWelcomeViewClick;
        AuthenticateButton.Click += AuthenticateClick;
        RevealPasswordButton.Click += RevealPasswordClick;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        LoginTextBox.Text = string.Empty;
        PasswordTextBox.Text = string.Empty;
        EyeCrossLine.IsVisible = false;

        HomeButton.Click -= ToWelcomeViewClick;
        AuthenticateButton.Click -= AuthenticateClick;
        RevealPasswordButton.Click -= RevealPasswordClick;

        _cancellationTokenSource?.Cancel();
    }
}