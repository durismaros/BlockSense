using BlockSense.Utilities.Logging;
using System;
using System.Security.Cryptography;

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
