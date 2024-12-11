using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
    /// Returns back to the MainView
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HomeClick(object sender, RoutedEventArgs e)
    {
        Content = new MainView();
    }

    private async void LoginClick(object sender, RoutedEventArgs e)
    {
        string? login = loginLogin.Text, password = passwordLogin.Text;

        try
        {

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                loginTextBorder.IsVisible = true;
                loginText.Text = "mandatory field is empty";
            }
            else if (loginLogin.Text != null && passwordLogin.Text != null)
            {
                var(loginCorrect, loginInfo) = await LoginUser(login, password);
                if (loginCorrect)
                {
                    loginTextBorder.IsVisible = true;
                    loginText.Text = "login successful";
                    ConsoleHelper.WriteLine("User logged in successfully");

                    await Task.Delay(2000);
                    Content = new Welcome();
                }
                else if (!loginCorrect)
                {
                    loginTextBorder.IsVisible = true;
                    loginText.Text = loginInfo;
                }
            }


        }
        catch (Exception ex)
        {
            loginTextBorder.IsVisible = true;
            loginText.Text = "An error occurred while processing your request";
            ConsoleHelper.WriteLine("Error: " + ex.Message);
        }
    }

    private async Task<(bool loginCorrect, string? loginInfo)> LoginUser(string login, string password)
    {
        using (var connection = await Database.GetConnectionAsync())
        {
            try
            {
                string command = "SELECT password_hash, salt FROM Users WHERE username = @login OR email = @login";
                var execute = new MySqlCommand(command, connection);
                execute.Parameters.AddWithValue("login", login);
                var reader = await execute.ExecuteReaderAsync();
                if (reader.Read())
                {
                    string correctHash = reader.GetString("password_hash");
                    string salt = reader.GetString("salt");
                    string inputHash = Hash.HashPassword(password, salt);
                    if (correctHash == inputHash)
                    {
                        return (true, null);
                    }
                    else
                    {
                        return (false, "Invalid password");
                    }
                }
                return (false, "username or email not found");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return (false, "An error occurred while processing your request");
            }
        }
    }

    private void ResetPasswordClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.google.com",
                UseShellExecute = true
            }
            );
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Error: " + ex.Message);
        }
    }
}