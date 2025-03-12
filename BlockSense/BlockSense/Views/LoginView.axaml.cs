using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using BlockSense.Client;
using BlockSense.Client_Side.Token_authentication;
using BlockSense.Server.User;
using BlockSense.Views;
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

    public LoginView()
    {
        InitializeComponent();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
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
        User.ResetPassword();
    }


    private void HomeClick(object sender, RoutedEventArgs e)
    {
        Animations.AnimateTransition(this, new MainView());
    }


    private async void LoginClick(object sender, RoutedEventArgs e)
    {

        if (User.Attempts >= 5)
        {
            SystemUtils.StartCheckTimer();
            ShowMessage("Try again later . . .");
            return;
        }

        string login = loginLogin.Text?.Trim() ?? string.Empty;
        string password = passwordLogin.Text?.Trim() ?? string.Empty;

        void ShowMessage(string message)
        {
            loginTextBorder.IsVisible = true;
            loginText.Text = message;
        }

        try
        {

            if (!InputHelper.Check(login, password))
            {
                ShowMessage("Looks like you missed a required field");
            }
            else
            {
                var(success, message) = await User.Login(login, password);
                if (success && !string.IsNullOrEmpty(message))
                {
                    ShowMessage(message);

                    await Task.Delay(2000);
                    Animations.AnimateTransition(this, new Welcome());
                }
                else if (!success && !string.IsNullOrEmpty(message))
                {
                    User.Attempts++;
                    ShowMessage(message);
                }
            }

        }

        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Error: " + ex.Message);
        }
    }
}