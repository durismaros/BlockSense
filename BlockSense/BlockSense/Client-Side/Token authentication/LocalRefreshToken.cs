using Avalonia.Controls.Platform;
using Avalonia.Remote.Protocol;
using BlockSense.DB;
using CredentialManagement;
using System;

namespace BlockSense.Client_Side.Token_authentication
{
    class LocalRefreshToken
    {
        public static void Store(string encryptedToken, string userId)
        {
            if (string.IsNullOrEmpty(encryptedToken))
                throw new ArgumentException("Encrypted token cannot be null or empty.", nameof(encryptedToken));

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));

            try
            {
                // Create a new Credential object
                var credential = new Credential
                {
                    Target = "BlockSense_RefreshToken",
                    Username = userId,           // Store the user ID as username
                    Password = encryptedToken,        // Store the encrypted token as password
                    Type = CredentialType.Generic,
                    PersistanceType = PersistanceType.LocalComputer // Secure storage for local machine
                };

                // Save the credential in the Windows Credential Store
                credential.Save();
                Console.WriteLine("Encrypted token and user ID successfully stored in Windows Credential Store.");
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during the storage process
                Console.WriteLine($"Error storing the encrypted token: {ex.Message}");
            }
        }

        public static (string userId, string refreshToken) Retrieve()
        {
            // Look for the credential that stores the user_id
            var credential = new Credential { Target = "BlockSense_RefreshToken" };
            credential.Load();

            if (credential.Exists())
            {
                // Retrieve the user_id (stored in the Username field)
                return (credential.Username, credential.Password);
            }
            return (string.Empty, string.Empty);
        }

        public static void Delete(string userId)
        {
            var credential = new Credential
            {
                Target = "BlockSense_RefreshToken",
                Username = userId
            };
            credential.Delete(); // Delete the credential for the specific user Id
        }
    }
}
