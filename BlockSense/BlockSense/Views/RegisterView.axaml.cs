using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlockSense.Views;
using MySql.Data.MySqlClient;
using Tmds.DBus.Protocol;
using ZstdSharp.Unsafe;

namespace BlockSense;

public partial class RegisterView : UserControl
{
    public RegisterView()
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

    /// <summary>
    /// 
    /// </summary>
    /// 
    private async void RegisterClick(object sender, RoutedEventArgs e)
    {
        string? username = usernameRegister.Text, email = emailRegister.Text, password = passwordRegister.Text, passwordconfirm = passwordConfirmRegister.Text, invitationcode = invitationCodeRegister.Text;
        try
        {

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordconfirm) || string.IsNullOrEmpty(invitationcode))
            {
                registerTextBorder.IsVisible = true;
                registerText.Text = "mandatory field is empty";
            }
            else if (!passwordRegister.Text.Equals(passwordConfirmRegister.Text))
            {
                registerTextBorder.IsVisible = true;
                registerText.Text = "passwords do not match";
            }
            else if (usernameRegister.Text != null && emailRegister.Text != null && invitationCodeRegister.Text != null)
            {
                await RegisterUser(username, email, password, invitationcode);
            }

        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Error: " + ex.Message);
        }
    }




    private async Task RegisterUser(string username, string email, string password, string invitationCode)
    {
        using (var connection = await Database.GetConnectionAsync())
        {
            try
            {
                // Check if the invitation code is valid and unused
                string command = "SELECT is_used FROM InvitationCodes WHERE code = @code";
                var execute = new MySqlCommand(command, connection);
                execute.Parameters.AddWithValue("@code", invitationCode);
                var result = await execute.ExecuteScalarAsync();

                if (result == null || (bool)result)
                {
                    string message = (result == null) ? "Invalid invitation code" : "Invitation code already used";
                    ConsoleHelper.WriteLine(message);

                    registerTextBorder.IsVisible = true;
                    registerText.Text = message;
                    
                    return;
                }

                // Generate salt and hash the password
                string salt = Hash.GenerateSalt();
                string hashedPassword = Hash.HashPassword(password, salt);

                // Insert user into the database
                command = "INSERT INTO Users (username, email, password_hash, salt, invitation_code) VALUES (@username, @email, @password_hash, @salt, @invitation_code)";
                execute = new MySqlCommand(command, connection);
                execute.Parameters.AddWithValue("@username", username);
                execute.Parameters.AddWithValue("@email", email);
                execute.Parameters.AddWithValue("@password_hash", hashedPassword);
                execute.Parameters.AddWithValue("@salt", salt);
                execute.Parameters.AddWithValue("@invitation_code", invitationCode);
                await execute.ExecuteNonQueryAsync();

                // Mark the invitation code as used
                command = "UPDATE InvitationCodes SET is_used = TRUE WHERE code = @code";
                execute = new MySqlCommand(command, connection);
                execute.Parameters.AddWithValue("@code", invitationCode);
                await execute.ExecuteNonQueryAsync();

                ConsoleHelper.WriteLine("User registered successfully");
                registerTextBorder.IsVisible = true;
                registerText.Text = "User registered successfully";
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine("Error: " + ex.Message);
                registerTextBorder.IsVisible = true;
                registerText.Text = "Error";
            }
        }

    }
}