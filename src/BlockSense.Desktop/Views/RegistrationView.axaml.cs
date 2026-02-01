using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BlockSense.Contracts.DTOs.Registration;
using BlockSense.Desktop.Services.Implementations;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.UIComponents;
using BlockSense.Desktop.Utilities.Validation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class RegistrationView : UserControl
{
    private readonly IUserService _userService;
    private readonly NavigationManager _navigationManager;

    private CancellationTokenSource? _cancellationTokenSource;

    public RegistrationView()
    {
        _userService = App.ServiceProvider.GetRequiredService<IUserService>()
            ?? throw new ArgumentNullException(nameof(IUserService));

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

        if (!DataAnnotationsValidator.TryValidate(request, out var validationError))
        {
            MainWindow.Instance.ShowNotification("Registration", validationError);
            return;
        }

        if (PasswordTextBox.Text != RepeatPasswordTextBox.Text)
        {
            MainWindow.Instance.ShowNotification("Registration", "Oops! Passwords do not match.");
            return;
        }

        await _userService.RegisterAsync(request, cancellationToken);
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

    private async void RevealRepeatedPasswordClick(object? sender, RoutedEventArgs e)
    {
        RepeatPasswordTextBox.PasswordChar = RepeatedEyeCrossLine.IsVisible ? '●' : '\0';

        if (RepeatedEyeCrossLine.IsVisible)
        {
            await Animations.FadeOutAnimation.RunAsync(RepeatedEyeCrossLine);
            RepeatedEyeCrossLine.IsVisible = false;
        }
        else
        {
            RepeatedEyeCrossLine.IsVisible = true;
            await Animations.FadeInAnimation.RunAsync(RepeatedEyeCrossLine);
        }
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _cancellationTokenSource?.Cancel();

        HomeButton.Click += ToWelcomeViewClick;
        RegisterButton.Click += RegisterClick;
        RevealPasswordButton.Click += RevealPasswordClick;
        RevealRepeatedPasswordButton.Click += RevealRepeatedPasswordClick;
    }

    private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UsernameTextBox.Text = string.Empty;
        EmailTextBox.Text = string.Empty;
        PasswordTextBox.Text = string.Empty;
        RepeatPasswordTextBox.Text = string.Empty;
        InvitationCodeTextBox.Text = string.Empty;
        EyeCrossLine.IsVisible = false;
        RepeatedEyeCrossLine.IsVisible = false;

        HomeButton.Click -= ToWelcomeViewClick;
        RegisterButton.Click -= RegisterClick;
        RevealPasswordButton.Click -= RevealPasswordClick;
        RevealRepeatedPasswordButton.Click -= RevealRepeatedPasswordClick;

        _cancellationTokenSource?.Cancel();
    }
}