using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using BlockSense.Client_Side.Token_authentication;
using BlockSense.Models.Requests;
using BlockSense.Services;
using BlockSense.Utilities;
using BlockSense.Views;
using DynamicData.Kernel;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Diagnostics;
using System.IO.Enumeration;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense;

public partial class LoginView : UserControl
{
    private readonly UserService _userService;
    private readonly IViewSwitcher _viewSwitcher;
    
    public LoginView(UserService userService, IViewSwitcher viewSwitcher)
    {
        _userService = userService;
        _viewSwitcher = viewSwitcher;
        InitializeComponent();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            LoginButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static void ResetPasswordClick(object sender, RoutedEventArgs e)
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


    private async void HomeClick(object sender, RoutedEventArgs e)
    {
        await _viewSwitcher.NavigateToAsync<MainView>();
    }


    private async void LoginClick(object sender, RoutedEventArgs e)
    {
        string login = loginLogin.Text?.Trim() ?? string.Empty;
        string password = passwordLogin.Text?.Trim() ?? string.Empty;

        async void ShowMessage(string message)
        {
            if (!loginTextBorder.IsVisible || loginText.Text != message)
            {
                loginText.Text = message;
                loginTextBorder.IsVisible = true;
                await AnimationManager.FadeInAnimation.RunAsync(loginTextBorder);
            }
        }

        try
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Looks like you missed a required field");
                return;
            }


            var request = new LoginRequestModel(login, password);
            var response = await _userService.Login(request);

            if (response is null || string.IsNullOrEmpty(response.Message))
                return;

            ShowMessage(response.Message);

            if (response.Success)
            {
                await Task.Delay(2000);
                await _viewSwitcher.NavigateToAsync<WelcomeView>();
            }

        }

        catch (Exception ex)
        {
            ConsoleLogger.Log("Error: " + ex.Message);
        }
    }
}