using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BlockSense.Models.Register;
using BlockSense.Services;
using BlockSense.Utilities.Logging;
using BlockSense.Utilities.UI;
using BlockSense.Views;

namespace BlockSense;

public partial class RegisterView : UserControl
{
    private readonly UserService _userService;
    private readonly AsyncDebouncer _debouncer;
    public RegisterView(UserService userService)
    {
        _userService = userService;

        _debouncer = new AsyncDebouncer();

        InitializeComponent();

        this.KeyDown += OnKeyDown;
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await _debouncer.TryExecuteAsync("register", Register);
    }


    /// <summary>
    /// Returns back to the MainView
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void HomeClick(object sender, RoutedEventArgs e)
    {
        //await _viewSwitcher.NavigateToAsync<MainView>();
    }

    private async void RegisterClick(object sender, RoutedEventArgs e)
    {
        await _debouncer.TryExecuteAsync("register", Register);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async Task Register()
    {
        string username = usernameRegister.Text?.Trim() ?? string.Empty;
        string email = emailRegister.Text?.Trim() ?? string.Empty;
        string password = passwordRegister.Text?.Trim() ?? string.Empty;
        string passwordConfirmation = passwordConfirmRegister.Text?.Trim() ?? string.Empty;
        string invitationCode = invitationCodeRegister.Text?.Trim() ?? string.Empty;

        async void ShowMessage(string message)
        {
            if (!registerTextBorder.IsVisible || registerText.Text != message)
            {
                registerText.Text = message;
                registerTextBorder.IsVisible = true;
                await AnimationManager.FadeInAnimation.RunAsync(registerTextBorder);
            }
        }

        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(passwordConfirmation) || string.IsNullOrWhiteSpace(invitationCode))
            {
                ShowMessage("Looks like you missed a required field");
                return;
            }

            if (password != passwordConfirmation)
            {
                ShowMessage("Passwords do not match");
                return;
            }

            var request = new RegisterRequest(username, email, password, invitationCode);
            var response = await _userService.Register(request);

            if (response is null || response.Message is null)
                return;

            ShowMessage(response.Message);
        }
        catch (Exception ex)
        {
            ConsoleLogger.Log("Error: " + ex.Message);
        }
    }
}