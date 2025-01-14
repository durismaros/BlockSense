using Avalonia.Data.Converters;
using BlockSense.Client_Side.Token_authentication;
using BlockSense.Server_Based.Cryptography;
using BlockSense.Server_Based.Cryptography.Token_authentication;
using BlockSense.Server_Based.Cryptography.Token_authentication.Refresh_Token;
using BlockSense.Server_Based.Cryptography.Tokens;
using BlockSense.Views;
using Org.BouncyCastle.Tls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.DB
{
    class User
    {
        private static int _uid;
        private static string _username = string.Empty, _email = string.Empty, _type = string.Empty, _creationDate = string.Empty, _invitingUser = string.Empty;
        protected static readonly string _deviceIdentifier = Environment.MachineName;
        protected static string _ipAddress = string.Empty;

        public static string deviceIdentifier
        {
            get { return _deviceIdentifier; }
        }

        public static string ipAddress
        {
            get
            {
                if (InputHelper.Check(_ipAddress))
                    return _ipAddress;

                return string.Empty;
            }
        }

        public static string Uid { get { return _uid.ToString(); } }
        public static string Username { get { return _username; } }
        public static string Email { get { return _email; } }
        public static string Type { get { return _type; } }
        public static string CreationDate { get { return _creationDate; } }
        public static string InvitingUser { get { return _invitingUser; } }


        public static async Task LoadUserInfo(string uid)
        {
            string query = "SELECT users.uid, users.username, users.email, users.type, users.password, users.salt, users.created_at, users1.username AS generated_by FROM users " +
                           "LEFT JOIN users as users1 ON users.invitation_code = users1.invitation_code WHERE users.uid = @uid";

            Dictionary<string, string> parameters = new()
            {
                {"@uid", uid}
            };
            using (var reader = await Database.FetchData(query, parameters))
            {
                if (reader.Read())
                {
                    _uid = Convert.ToInt32(uid);
                    _username = reader.GetString("username");
                    _email = reader.GetString("email");
                    _type = reader.GetString("type");
                    _creationDate = reader.GetDateTime("created_at").ToString("yyyy-MM-dd HH:mm:ss");
                    _invitingUser = reader.GetString("generated_by");
                }
            }

        }

        public static void DeleteUserInfo()
        {
            _uid = -1;
            _username = string.Empty;
            _email = string.Empty;
            _type = string.Empty;
            _creationDate = string.Empty;
            _invitingUser = string.Empty;
        }


        public static async Task<(bool correctLogin, string? loginMessage)> LoginUser(string login, string password)
        {
            Dictionary<string, string> parameters = new()
            {
                {"@login", login}
            };

            string query = "SELECT uid, username, email, password, salt FROM users WHERE username = @login || email = @login";

            try
            {
                using (var reader = await Database.FetchData(query, parameters))
                {
                    if (reader.Read())
                    {
                        string correctHash = reader.GetString("password");
                        string salt = reader.GetString("salt");
                        // Combine hashed password with the salt
                        string inputHash = HashUtils.GenerateHash(password, salt);
                        if (correctHash == inputHash)
                        {
                            string uid = reader.GetInt32("uid").ToString();

                            // Server related
                            string refreshToken = RemoteRefreshToken.GeneratePlain();
                            await RemoteRefreshToken.Store(uid, refreshToken);

                            // Client Related
                            var (encryptionKey, initializationVector) = await EncryptionKey.Fetch(uid);
                            string encryptedRefreshToken = TokenEncryption.EncryptRefreshToken(refreshToken, encryptionKey, initializationVector);
                            LocalRefreshToken.Store(encryptedRefreshToken, uid);

                            await User.LoadUserInfo(uid);
                            ConsoleHelper.WriteLine("User logged in successful");
                            return (true, "Welcome back! You’re all set.");
                        }
                        else
                        {
                            ConsoleHelper.WriteLine("Invalid password");
                            return (false, "Oops! Wrong password entered.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("User was not found");
                        return (false, "Hmm, we couldn’t find your account.");
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine("Error: " + ex.Message);
                return (false, "Something went wrong.");
            }
        }


        public static async Task<(bool correctRegister, string? registerMessage)> RegisterUser(string username, string email, string password, string invitationCode)
        {
            try
            {
                // Check if the invitation code is valid and unused
                if (await InvitationCheck(invitationCode))
                {
                    // Generate salt and hash the password
                    string salt = HashUtils.GenerateSalt();
                    string hashedPassword = HashUtils.GenerateHash(password, salt);

                    // Add parameters from user TextBoxex
                    Dictionary<string, string> parameters = new()
                    {
                        {"@username", username},
                        {"@email", email},
                        {"@password", hashedPassword},
                        {"@salt", salt},
                        {"@invitation_code", invitationCode}
                    };

                    // Insert user into the database
                    string query = "INSERT INTO users (username, email, password, salt, invitation_code) VALUES (@username, @email, @password, @salt, @invitation_code)";
                    if (await Database.StoreData(query, parameters))
                    {
                        // Set the invitation code as used
                        query = "UPDATE InvitationCodes SET is_used = TRUE WHERE invitation_code = @invitation_code";
                        await Database.StoreData(query, parameters);

                        query = "SELECT LAST_INSERT_ID() AS uid";
                        using (var reader = await Database.FetchData(query, parameters))
                        {
                            if (reader.Read())
                            {
                                string uid = reader.GetInt16("uid").ToString();

                                // Server Related
                                string encryptionKey = EncryptionKey.Generate(hashedPassword, salt);
                                await EncryptionKey.DatabaseStore(uid, encryptionKey);
                            }
                        }

                        ConsoleHelper.WriteLine($"User registered successfully");
                        return (true, "Registration complete. Welcome to BlockSense.");
                    }

                    // Request not handled correctly
                    Console.WriteLine("There has been a problem with storing user into DB");
                    return (false, "We couldn’t register your account.");
                }
                return (false, "The invitation code doesn’t seem right.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine("Error: " + ex.Message);
                return (false, "Something went wrong.");
            }
        }

        public static async Task Logout()
        {
            LocalRefreshToken.Delete(User.Uid);
            await RemoteRefreshToken.Revoke(User.Uid);
            User.DeleteUserInfo();
        }

        private static async Task<bool> InvitationCheck(string invitationCode)
        {
            Dictionary<string, string> parameters = new()
            {
                {"@invitation_code", invitationCode}
            };
            string query = "SELECT is_used FROM invitationcodes WHERE invitation_code = @invitation_code";
            try
            {
                using (var reader = await Database.FetchData(query, parameters))
                {
                    if (reader.Read())
                    {
                        bool isUsed = reader.GetBoolean("is_used"); // Get the is_used value (0[T] or 1[F])
                        if (!isUsed)
                        {
                            return true; // If invitation code is valid and has not been used
                        }

                        Console.WriteLine("Invitation code is not available");
                        return false; // Already used inviation code
                    }

                    Console.WriteLine("Invalid invitation code");
                    return false; // If no record is found for the invitation code
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine("Error: " + ex.Message);
                return false;
            }
        }


        public static void ResetPassword()
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
                ConsoleHelper.WriteLine("Error: " + ex.Message);
            }
        }


        public static async Task GetIPAddress()
        {
            using (var httpClient = new HttpClient())
            {
                _ipAddress = await httpClient.GetStringAsync("https://api64.ipify.org");
            }
        }
    }
}
