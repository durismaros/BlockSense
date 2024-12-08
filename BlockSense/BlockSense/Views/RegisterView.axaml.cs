using System;
using System.Security.Cryptography.X509Certificates;
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
    private void RegisterClick(object sender, RoutedEventArgs e)
    {
        string? username = usernameRegister.Text, email = emailRegister.Text, password = passwordRegister.Text, passwordconfirm = passwordConfirmRegister.Text, invitationcode = invitationCodeRegister.Text;
        try
        {

            if (username == null || email == null || password == null || passwordconfirm == null || invitationcode == null)
            {
                registerText.Text = "mandatory field is empty";
                registerTextBorder.IsVisible = true;
            }
            else if (passwordRegister.Text != passwordConfirmRegister.Text)
            {
                registerText.Text = "passwords do not match";
                registerTextBorder.IsVisible = true;
            }
            else if (usernameRegister.Text != null && emailRegister.Text != null && invitationCodeRegister.Text != null)
            {
                RegisterUser(username, email, password, invitationcode);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }



    private void RegisterUser(string username, string email, string password, string invitationCode)
    {
        using (var connection = Database.GetConnection())  // Use the shared connection from Database class
        {
            try
            {
                // Check if the invitation code is valid and unused
                string query = "SELECT is_used FROM InvitationCodes WHERE code = @code";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@code", invitationCode);
                var result = command.ExecuteScalar();

                if (result == null || (bool)result)
                {
                    string message = (result == null) ? "Invalid invitation code." : "Invitation code already used.";
                    Console.WriteLine(message);

                    registerTextBorder.IsVisible = true;
                    registerText.Text = message;
                    
                    return;
                }

                // Generate salt and hash the password
                string salt = Hash.GenerateSalt();
                string hashedPassword = Hash.HashPassword(password, salt);

                // Insert user into the database
                query = "INSERT INTO Users (username, email, password_hash, salt, invitation_code) VALUES (@username, @email, @password_hash, @salt, @invitation_code)";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@password_hash", hashedPassword);
                command.Parameters.AddWithValue("@salt", salt);
                command.Parameters.AddWithValue("@invitation_code", invitationCode);
                command.ExecuteNonQuery();

                // Mark the invitation code as used
                query = "UPDATE InvitationCodes SET is_used = TRUE WHERE code = @code";
                command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@code", invitationCode);
                command.ExecuteNonQuery();

                Console.WriteLine("User registered successfully.");
                registerTextBorder.IsVisible = true;
                registerText.Text = "User registered successfully";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                registerTextBorder.IsVisible = true;
                registerText.Text = "Error";
            }
        }

    }
}