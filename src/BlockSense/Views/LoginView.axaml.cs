using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BlockSense.Models.Login;
using BlockSense.Models.TwoFactorAuth.Setup;
using BlockSense.Models.TwoFactorAuth.Verification;
using BlockSense.Models.User;
using BlockSense.Services;
using BlockSense.Utilities.Logging;
using BlockSense.Utilities.UI;
using BlockSense.Views;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BlockSense;

public partial class LoginView : UserControl
{
    private readonly UserService _userService;
    private readonly IViewSwitcher _viewSwitcher;

    private readonly TwoFactorSlidingPanel _twoFactorSlidingPanel;

    private readonly AsyncDebouncer _debouncer;


    public LoginView(UserService userService, IViewSwitcher viewSwitcher, TwoFactorSlidingPanel twoFactorSlidingPanel)
    {
        _userService = userService;
        _viewSwitcher = viewSwitcher;
        _twoFactorSlidingPanel = twoFactorSlidingPanel;

        _debouncer = new AsyncDebouncer();

        InitializeComponent();

        MainPanel.Children.Add(_twoFactorSlidingPanel);
        _twoFactorSlidingPanel.CodeSubmitted += OnTwoFactorCodeSubmitted;

        this.KeyDown += OnKeyDown;
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Use the debouncer with a unique key for login operations
        if (e.Key == Key.Enter && !_twoFactorSlidingPanel.IsPanelVisible)
            await _debouncer.TryExecuteAsync("login", Login);
    }

    private async void HomeClick(object sender, RoutedEventArgs e)
    {
        await _viewSwitcher.NavigateToAsync<MainView>();
    }


    private async void LoginClick(object sender, RoutedEventArgs e)
    {
        await _debouncer.TryExecuteAsync("login", Login);
    }

    private async Task Login()
    {
        string login = loginLogin.Text?.Trim() ?? string.Empty;
        string password = passwordLogin.Text?.Trim() ?? string.Empty;

        try
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Looks like you missed a required field");
                return;
            }


            var request = new LoginRequest(login, password);
            var response = await _userService.Login(request);

            if (response is null || string.IsNullOrEmpty(response.Message))
                return;

            if (response.TwoFactorRequired)
                await _twoFactorSlidingPanel.ShowPanel(TwoFactorSlidingPanel.TwoFactorMode.Verify);

            ShowMessage(response.Message);

            if (!response.Success)
                return;

            await Task.Delay(2000);
            await _viewSwitcher.NavigateToAsync<WelcomeView>();
        }

        catch (Exception ex)
        {
            ConsoleLogger.Log("Error: " + ex.Message);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void ResetPasswordClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.google.com/",
                UseShellExecute = true
            }
            );
        }
        catch (Exception ex)
        {
            ConsoleLogger.Log("Error: " + ex.Message);
        }
    }

    private async void OnTwoFactorCodeSubmitted(object? sender, TwoFactorCodeEventArgs e)
    {
        var executed = await _debouncer.TryExecuteAsync("verifyTwoFa", async () =>
        {
            await HandleCodeVerification(e);
        });
    }

    private async Task HandleCodeVerification(TwoFactorCodeEventArgs e)
    {
        try
        {

            string login = loginLogin.Text?.Trim() ?? string.Empty;
            string password = passwordLogin.Text?.Trim() ?? string.Empty;

            var request = new LoginRequest(login, password, e.Code);
            var response = await _userService.Login(request);

            if (response is null || !response.Success || string.IsNullOrEmpty(response.Message))
            {
                await _twoFactorSlidingPanel.ShowError();
                return;
            }

            ShowMessage(response.Message);
            await _twoFactorSlidingPanel.ShowSuccessState();

            await Task.Delay(1000);
            await _viewSwitcher.NavigateToAsync<WelcomeView>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verifying 2FA code: {ex.Message}");
            await _twoFactorSlidingPanel.ShowError();
        }
    }

    private async void ShowMessage(string message)
    {
        if (!loginTextBorder.IsVisible || loginText.Text != message)
        {
            loginText.Text = message;
            loginTextBorder.IsVisible = true;
            await AnimationManager.FadeInAnimation.RunAsync(loginTextBorder);
        }
    }
}