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
using BlockSense.Client;
using BlockSense.Client.Utilities;
using BlockSense.Client_Side.Token_authentication;
using BlockSense.Server;
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
            ConsoleHelper.Log("Error: " + ex.Message);
        }
    }


    private async void HomeClick(object sender, RoutedEventArgs e)
    {
        await MainWindow.SwitchView(new MainView());
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
                await Animations.FadeInAnimation.RunAsync(loginTextBorder);
            }
        }

        try
        {

            if (!SystemUtils.CheckTimeOut())
            {
                ShowMessage("Try again later . . .");
                return;
            }

            else if (!InputHelper.Check(login, password))
                ShowMessage("Looks like you missed a required field");


            else
            {
                var (success, message) = await User.Login(login, password);
                if (success && !string.IsNullOrEmpty(message))
                {
                    ShowMessage(message);

                    await Task.Delay(2000);
                    await MainWindow.SwitchView(new Welcome());
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
            ConsoleHelper.Log("Error: " + ex.Message);
        }
    }
}