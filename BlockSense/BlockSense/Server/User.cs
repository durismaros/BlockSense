using Avalonia.Metadata;
using BlockSense.Client;
using BlockSense.Client.Cryptography;
using BlockSense.Client.Cryptography.Hashing;
using BlockSense.Client.DataProtection;
using BlockSense.Client.Utilities;
using BlockSense.Client_Side.Token_authentication;
using BlockSense.DatabaseUtils;
using BlockSense.Server.Cryptography.TokenAuthentication;
using NBitcoin.RPC;
using Org.BouncyCastle.Math.EC.Endo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace BlockSense.Server
{
    class User
    {
        private readonly ApiClient _apiClient;
        private readonly SystemIdentifiers _identifiers;
        public User()
        {
            _apiClient = new ApiClient();
            _identifiers = new SystemIdentifiers();
        }

        private static int _uid;
        private static string _username = string.Empty;
        private static string _email = string.Empty;
        private static UserType _type = UserType.User;
        private static string _invitingUser = string.Empty;
        private static DateTime _createdAt;
        private static DateTime _updatedAt;
        private static string _ipAddress = string.Empty;

        public static class AdditionalInformation
        {
            public static int InvitedUsers {  get; private set; }
            public static int ActiveDevices { get; private set; }

            public static async Task LoadAdditionalUserInfo()
            {
                await GetInvitedUsers();
                await GetActiveDevices();
            }
            private static async Task GetInvitedUsers()
            {
                string query = "select count(invitation_id) as invitedUsers from invitationcodes where generated_by = @uid and is_used = 1 group by generated_by";
                Dictionary<string, object> parameters = new()
                {
                    {"@uid", User.Uid}
                };

                using (var reader = await Database.FetchData(query, parameters))
                {
                    if (reader.Read())
                        InvitedUsers = reader.GetInt32("invitedUsers");
                }
            }

            private static async Task GetActiveDevices()
            {
                string query = "select count(distinct hardware_identifier) as active_devices from refreshtokens where user_id = @uid and revoked = 0 and expires_at > now()";
                Dictionary<string, object> parameters = new()
                {
                    {"@uid", User.Uid}
                };
                using (var reader = await Database.FetchData(query, parameters))
                {
                    if (reader.Read())
                        ActiveDevices = reader.GetInt32("active_devices");

                }
            }

        }
        public static int Attempts { get; set; } = 0;
        public static string DeviceIdentifier { get; } = Environment.MachineName;
        public static string IpAddress
        {
            get => InputHelper.Check(_ipAddress) ? _ipAddress : string.Empty;
            set => _ipAddress = value;
        }
        public static int Uid => _uid;
        public static string Username => _username;
        public static string Email => _email;
        public static UserType Type => _type;
        public static DateTime CreatedAt => _createdAt;
        public static DateTime UpdatedAt => _updatedAt;
        public static string InvitingUser => _invitingUser;

        /// <summary>
        /// Loads a basic user information into memory
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public static async Task LoadUserInfo(int uid)
        {
            string query = "select users.uid, users.username, users.email, users.type, users.password, users.salt, users.created_at, users.updated_at, users1.username as generated_by from users " +
                "join invitationcodes ON users.invitation_code = invitationcodes.invitation_id left join users AS users1 ON invitationcodes.generated_by = users1.uid where users.uid = @uid";

            Dictionary<string, object> parameters = new()
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
                    if (Enum.TryParse(reader.GetString("type"), true, out UserType type))
                        _type = type;

                    _createdAt = reader.GetDateTime("created_at");
                    _updatedAt = reader.GetDateTime("updated_at");
                    _invitingUser = reader.GetString("generated_by");

                    await AdditionalInformation.LoadAdditionalUserInfo();
                    ConsoleHelper.Log("User's data fetched successfully");
                }
            }
        }

        public static void EraseUserData()
        {
            _uid = 0;
            _username = string.Empty;
            _email = string.Empty;
            _type = UserType.User;
            _createdAt = DateTime.MinValue;
            _updatedAt = DateTime.MinValue;
            _invitingUser = string.Empty;
        }


        public async Task<(bool correctLogin, string loginMessage)> Login(string login, string password)
        {
            var loginResponse = await _apiClient.Login(login, password, _identifiers);

            if (loginResponse.Success)
            {
                byte[] entropy = EntropyManager.StoreEntropy();


                // Store the token securely
                SecureTokenManager.StoreToken(loginResponse.TokenData, entropy);

                // Load user info
                var userInfo = await _apiClient.LoadUserInfo(loginResponse.TokenData.UserId);
                // Update your UI with user info
            }

            return (loginResponse.Success, loginResponse.Message);
        }


        public static async Task<(bool correctRegister, string registerMessage)> Register(string username, string email, string password, string invitationCode)
        {
            // Check if the invitation code is valid and unused
            if (await InvitationCheck(invitationCode))
            {
                // Generate salt and hash the password
                byte[] salt = CryptoUtils.SecureRandomGenerator();
                byte[] hashedPassword = HashUtils.ComputeSha256(Encoding.UTF8.GetBytes(password), salt);

                // Add parameters from user TextBoxex
                Dictionary<string, object> parameters = new()
                    {
                        {"@username", username},
                        {"@email", email},
                        {"@password", hashedPassword},
                        {"@salt", salt},
                        {"@invitation_code", invitationCode}
                    };

                // Insert user into the database
                string query = "insert into users (username, email, password, salt, invitation_code) values (@username, @email, @password, @salt, (select invitation_id from invitationcodes where invitation_code = @invitation_code))";
                if (await Database.StoreData(query, parameters))
                {
                    // Set the invitation code as used
                    query = "update InvitationCodes set is_used = TRUE where invitation_code = @invitation_code";
                    await Database.StoreData(query, parameters);

                    ConsoleHelper.Log("User registered and added to database successfully");
                    return (true, "Registration complete! Welcome");
                }

                // Request not handled correctly
                Console.WriteLine("There has been a problem with storing user into DB");
                return (false, "We couldn’t register your account");
            }
            return (false, "Invitation code doesn’t seem right");
        }

        public static async Task Logout()
        {
            //SecureTokenStorage.Delete();
            //await TokenUtils.Revoke(RemoteRefreshToken.TokenId);
            EraseUserData();
        }

        private static async Task<bool> InvitationCheck(string invitationCode)
        {
            Dictionary<string, object> parameters = new()
            {
                {"@invitation_code", invitationCode}
            };
            string query = "select invitation_code from invitationcodes where invitation_code = @invitation_code and is_used = false and revoked = false and expires_at > now()";
            try
            {
                using (var reader = await Database.FetchData(query, parameters))
                {
                    if (reader.Read())
                    {
                        return true;
                    }

                    ConsoleHelper.Log("Invalid invitation code");
                    return false; // If no record is found for the invitation code
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
                return false;
            }
        }

        public enum UserType
        {
            User,
            Admin,
            Banned
        }
    }
}