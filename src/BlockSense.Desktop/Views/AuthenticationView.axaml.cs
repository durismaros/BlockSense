using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Contracts.Definitions;
using BlockSense.Contracts.DTOs.Authentication;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using BlockSense.Desktop.Utilities.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class AuthenticationView : UserControl
{
    private readonly IAuthService _authService;
    private readonly NavigationManager _navigationManager;
    private readonly TwoFactorSlidingPanel _twoFactorSlidingPanel;

    private string? _twoFactorCode;
    private CancellationTokenSource? _cancellationTokenSource;

    public AuthenticationView(IAuthService authService, NavigationManager navigationManager)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
        _twoFactorSlidingPanel = MainWindow.Instance.TwoFactorSlidingPanel ?? throw new ArgumentNullException(nameof(MainWindow.Instance.TwoFactorSlidingPanel));

        _twoFactorSlidingPanel.TwoFactorCodeSubmitted += async code =>
        {
            _twoFactorCode = code;
            AuthenticateClick();
        };

        InitializeComponent();

        HomeButton.Click += ToWelcomeViewClick;
        AuthenticateButton.Click += AuthenticateClick;
        RevealPasswordButton.Click += RevealPasswordClick;
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
            TwoFactorCode = _twoFactorCode ?? string.Empty
        };

        if (!DataAnnotationsValidator.TryValidate(request, out var validationError))
        {
            ShowOutput(validationError);
            return;
        }

        var authTask = _authService.AuthAsync(request, cancellationToken);
        var delayTask = Task.Delay(1000, cancellationToken);

        // Wait for whichever finishes first
        var completedTask = await Task.WhenAny(authTask, delayTask);

        if (completedTask == delayTask)
        {
            ShowOutput("Authenticating . . .");
        }

        var response = await authTask;

        switch (response.ProblemType)
        {
            case ApiProblemTypes.Authentication.AuthenticationSuccess:
                _twoFactorSlidingPanel.HidePanel();

                ShowOutput(response.Message);
                await Task.Delay(2000);

                await _navigationManager.NavigateToAsync<HomeView>();

                LoginTextBox.Text = string.Empty;
                PasswordTextBox.Text = string.Empty;
                OutputTextBlock.Text = string.Empty;
                OutputBorder.IsVisible = false;
                break;

            case ApiProblemTypes.Authentication.TwoFactorRequired:
                _twoFactorSlidingPanel.ShowPanel();
                break;

            case ApiProblemTypes.TwoFactorAuthentication.InvalidCode:
                await _twoFactorSlidingPanel.ShowErrorState();
                break;

            default:
                ShowOutput(response.Message);
                break;
        }
    }

    private async void RevealPasswordClick(object? sender, RoutedEventArgs e)
    {
        PasswordTextBox.PasswordChar = EyeCrossLine.IsVisible ? '●' : '\0';

        if (EyeCrossLine.IsVisible)
        {
            // Password revealed → remove the cross line
            await Animations.FadeOutAnimation.RunAsync(EyeCrossLine);
            EyeCrossLine.IsVisible = false;
        }
        else
        {
            // Password hidden → show the cross line
            EyeCrossLine.IsVisible = true;
            await Animations.FadeInAnimation.RunAsync(EyeCrossLine);
        }
    }

    /// <summary>
    /// Displays a message to the user in the output panel with a fade-in animation.
    /// </summary>
    /// <param name="message">The message text to display.</param>
    private async void ShowOutput(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (OutputTextBlock.Text == message)
            return;

        OutputTextBlock.Text = message;
        await Animations.FadeInAnimation.RunAsync(OutputTextBlock);

        if (OutputBorder.IsVisible)
            return;

        OutputBorder.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(OutputBorder);
    }
}