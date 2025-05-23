using BlockSense.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.auth.DataProtection
{
    class WinDataProtection
    {
        public static byte[]? Encrypt(byte[] data, byte[]? entropy = null)
        {
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("Data protection is only supported on Windows");

            try
            {
                return ProtectedData.Protect(
                    data,
                    entropy,
                    DataProtectionScope.CurrentUser);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log("Error: " + ex.Message);
                return null;
            }
        }

        public static byte[]? Decrypt(byte[] encryptedData, byte[]? entropy = null)
        {
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("Data protection is only supported on Windows");

            try
            {
                return ProtectedData.Unprotect(
                    encryptedData,
                    entropy,
                    DataProtectionScope.CurrentUser);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log("Error: " + ex.Message);
                return null;
            }
        }
    }
}
