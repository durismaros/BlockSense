using BlockSense.Cryptography;
using BlockSense.Cryptography.Hashing;
using BlockSense.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.auth.DataProtection
{
    class EntropyManager
    {
        private const string REGISTRY_PATH = @"HKEY_CURRENT_USER\Software\BlockSense\Security";
        private const string ENTROPY_VALUE_NAME = "EncryptedEntropy";

        public static void StoreEntropy()
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                {
                    throw new PlatformNotSupportedException("Data protection is only supported on Windows");
                }

                byte[] sourceEntropy = CryptographyUtils.SecureRandomGenerator(32);
                byte[] base64Entropy = HashingFunctions.ComputeSha256(sourceEntropy);

                // Encrypt the entropy with DPAPI
                byte[]? encryptedEntropy = WinDataProtection.Encrypt(base64Entropy);

                if (encryptedEntropy is null)
                    return;

                // Save to registry
                Registry.SetValue(
                    REGISTRY_PATH,
                    ENTROPY_VALUE_NAME,
                    Convert.ToBase64String(encryptedEntropy),
                    RegistryValueKind.String);

            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
            }
        }

        public static byte[]? RetrieveEntropy()
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                {
                    throw new PlatformNotSupportedException("Data protection is only supported on Windows");
                }

                string encryptedBase64 = (string)Registry.GetValue(REGISTRY_PATH, ENTROPY_VALUE_NAME, null)!;

                if (!InputHelper.Check(encryptedBase64))
                {
                    ConsoleHelper.Log("No entropy found in registry");
                    return null;
                }

                byte[] encryptedEntropy = Convert.FromBase64String(encryptedBase64);

                return WinDataProtection.Decrypt(encryptedEntropy);
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
                return null;
            }
        }
    }
}
