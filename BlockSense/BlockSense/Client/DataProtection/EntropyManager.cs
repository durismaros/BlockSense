using BlockSense.Client.Cryptography;
using BlockSense.Client.Cryptography.Hashing;
using BlockSense.Client.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Client.DataProtection
{
    class EntropyManager
    {
        private const string REGISTRY_PATH = @"HKEY_CURRENT_USER\Software\BlockSense\Security";
        private const string ENTROPY_VALUE_NAME = "EncryptedEntropy";

        public static byte[] StoreEntropy()
        {
            try
            {
                if (!OperatingSystem.IsWindows())
                {
                    throw new PlatformNotSupportedException("Data protection is only supported on Windows");
                }

                byte[] sourceEntropy = CryptoUtils.SecureRandomGenerator(32);
                byte[] base64Entropy = HashUtils.ComputeSha256(sourceEntropy);

                // Encrypt the entropy with DPAPI
                byte[] encryptedEntropy = WinDPAPI.Encrypt(base64Entropy);

                // Save to registry
                Registry.SetValue(
                    REGISTRY_PATH,
                    ENTROPY_VALUE_NAME,
                    Convert.ToBase64String(encryptedEntropy),
                    RegistryValueKind.String);

                return base64Entropy;

            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
                return Array.Empty<byte>();
            }
        }

        public static byte[] RetrieveEntropy()
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
                    return Array.Empty<byte>();
                }

                byte[] encryptedEntropy = Convert.FromBase64String(encryptedBase64);

                return WinDPAPI.Decrypt(encryptedEntropy);
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
                return Array.Empty<byte>();
            }
        }
    }
}
