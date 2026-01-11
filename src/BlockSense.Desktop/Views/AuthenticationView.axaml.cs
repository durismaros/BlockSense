using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using BlockSense.Desktop.Utilities.Validation;
using System;
using System.Threading;

namespace BlockSense.Desktop;

public partial class AuthenticationView : UserControl
{
    private readonly NavigationManager _navigationManager;
    private readonly TwoFactorSlidingPanel _twoFactorSlidingPanel;
    private readonly IAuthService _authService;

    private string? _twoFactorCode;
    private CancellationTokenSource? _cancellationTokenSource;

    public AuthenticationView(NavigationManager navigationManager, TwoFactorSlidingPanel twoFactorSlidingPanel, IAuthService authService)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _twoFactorSlidingPanel = twoFactorSlidingPanel ?? throw new ArgumentNullException(nameof(twoFactorSlidingPanel));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));

        _twoFactorSlidingPanel.TwoFactorCodeSubmitted += async code =>
        {
            _twoFactorCode = code;
            AuthenticateClick(default, default);
        };

        InitializeComponent();

        if (Content is Panel panel)
        {
            panel.Children.Add(_twoFactorSlidingPanel);
        }

        HomeButton.Click += ToWelcomeViewClick;
        AuthenticateButton.Click += AuthenticateClick;

        _twoFactorSlidingPanel.ShowPanel(default, default);
    }

    private async void ToWelcomeViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WelcomeView>();
    }

    private async void AuthenticateClick(object? sender, RoutedEventArgs? e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        var request = new AuthRequest
        {
            Login = LoginTextBox.Text?.Trim() ?? string.Empty,
            Password = PasswordTextBox.Text ?? string.Empty,
            TwoFactorCode = _twoFactorCode ?? string.Empty,
        };

        if (!DataAnnotationsValidator.TryValidate(request, out var validationError))
        {
            ShowOutput(validationError);
            return;
        }

        var response = await _authService.AuthAsync(request, cancellationToken);

        switch (response.ProblemType)
        {
            case ApiProblemTypes.Authentication.AuthenticationSuccess:
                ShowOutput(response.Message);
                Thread.Sleep(500);

                await _navigationManager.NavigateToAsync<WelcomeView>();
                break;

            case ApiProblemTypes.Authentication.TwoFactorRequired:
                break;

            default:
                ShowOutput(response.Message);
                break;
        }
    }

    /// <summary>
    /// Displays a message to the user in the output panel with a fade-in animation.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    private async void ShowOutput(string message)
    {
        OutputTextBlock.Text = message;
        OutputBorder.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(OutputBorder);
    }
}