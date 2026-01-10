using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using System;
using System.Threading;

namespace BlockSense.Desktop;

public partial class RegistrationView : UserControl
{
    private readonly IUserService _userService;
    private readonly NavigationManager _navigationManager;

    private CancellationTokenSource? _cancellationTokenSource;

    public RegistrationView(IUserService userService, NavigationManager navigationManager)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

        InitializeComponent();

        HomeButton.Click += ToWelcomeViewClick;
        RegisterButton.Click += RegisterClick;
    }

    private async void ToWelcomeViewClick(object? sender, RoutedEventArgs e)
    {
        await _navigationManager.NavigateToAsync<WelcomeView>();
    }

    private async void RegisterClick(object? sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        var request = new RegistrationRequest
        {
            Username = UsernameTextBox.Text?.Trim() ?? string.Empty,
            Email = EmailTextBox.Text?.Trim().ToLowerInvariant() ?? string.Empty,
            Password = PasswordTextBox.Text ?? string.Empty,
            InvitationCode = InvitationCodeTextBox.Text?.Trim() ?? string.Empty
        };


        if (!ValidatorService.TryValidate(request, out var validationError))
        {
            ShowOutput(validationError);
            return;
        }

        if (request.Password != ConfirmPasswordTextBox.Text)
        {
            ShowOutput("Passwords Do Not Match");
            return;
        }

        var response = await _userService.RegisterAsync(request, cancellationToken);

        ShowOutput(response.Message);
    }

    private async void ShowOutput(string message)
    {
        OutputTextBlock.Text = message;
        OutputBorder.IsVisible = true;
        await Animations.FadeInAnimation.RunAsync(OutputBorder);
    }
}