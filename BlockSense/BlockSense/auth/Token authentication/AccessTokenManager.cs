using BlockSense.auth.DataProtection;
using BlockSense.Models.Token;
using BlockSense.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Client.Token_authentication
{
    public class AccessTokenManager
    {
        public AccessTokenModel? tokenCache;

        public void StoreToken(AccessTokenModel token)
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                    throw new PlatformNotSupportedException("Data protection is only supported on Windows");

                byte[]? entropy = EntropyManager.RetrieveEntropy();

                if (token is null || token.Data is null || entropy is null)
                {
                    ConsoleHelper.Log("Either token or entropy is empty");
                    return;
                }

                var encryptedData = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(token.Data),
                    entropy,
                    DataProtectionScope.CurrentUser);

                tokenCache = new()
                {
                    Data = Convert.ToBase64String(encryptedData),
                    ExpiresIn = token.ExpiresIn
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token storage failed: {ex.Message}");
            }
        }

        public string? RetrieveToken()
        {
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("Data protection is only supported on Windows");

            byte[]? entropy = EntropyManager.RetrieveEntropy();

            if (tokenCache is null || tokenCache.Data is null || entropy is null)
            {
                ConsoleHelper.Log("Either token or entropy is empty");
                return null;
            }

            try
            {
                var encryptedData = Convert.FromBase64String(tokenCache.Data);
                var decryptedData = ProtectedData.Unprotect(
                    encryptedData,
                    entropy,
                    DataProtectionScope.CurrentUser);

                return Encoding.UTF8.GetString(decryptedData);
            }
            catch (Exception ex)
            {
                // Handle decryption failure
                Console.WriteLine($"Token retrieval failed: {ex.Message}");
                return null;
            }
        }

        // Clear token from memory
        public void ClearToken()
        {
            if (tokenCache is not null)
            {
                // Overwrite the memory
                //SecureWiper.Wipe(ref _tokenCache.Data);
                tokenCache = null;
            }
        }
    }
}
