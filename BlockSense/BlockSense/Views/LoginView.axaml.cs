using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
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
        Content = new MainView();
    }

    private async void LoginClick(object sender, RoutedEventArgs e)
    {
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
                ShowMessage("Looks like you missed a required field.");
            }
            else
            {
                var(correctLogin, loginMessage) = await User.Login(login, password);
                if (correctLogin && !string.IsNullOrEmpty(loginMessage))
                {
                    ShowMessage(loginMessage);

                    await Task.Delay(2000);
                    Content = new Welcome();
                }
                else if (!correctLogin && !string.IsNullOrEmpty(loginMessage))
                {
                    ShowMessage(loginMessage);
                }
            }

        }

        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Error: " + ex.Message);
        }
    }
}