using Avalonia.Controls.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Client
{
    class DirStructure
    {
        public static readonly string BaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BlockSense");
        public static readonly string authPath = Path.Combine(BaseDirectory, "auth");
        public static readonly string walletPath = Path.Combine(BaseDirectory, "blocksense.wallet");
        public static readonly string logsPath = Path.Combine(BaseDirectory, "logs");

        public static void InitializeSecureStorage()
        {
            try
            {
                CreateBaseDirectory();
                CreateAuthStructure();
                CreateLogsStructure();
                CreateWalletStructure();
                ConsoleHelper.Log("Storage structure initialized");
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
            }
        }
        private static void CreateBaseDirectory()
        {
            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);

                // Mark base directory as hidden
                File.SetAttributes(BaseDirectory, FileAttributes.Hidden | FileAttributes.NotContentIndexed);
            }
        }

        private static void CreateAuthStructure()
        {
            if (!Directory.Exists(authPath))
            {
                Directory.CreateDirectory(authPath);

                // Mark auth directory as hidden
                File.SetAttributes(authPath, FileAttributes.Hidden | FileAttributes.NotContentIndexed);
            }
        }

        private static void CreateLogsStructure()
        {
            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
            }
        }

        private static void CreateWalletStructure()
        {
            if (!Directory.Exists(walletPath))
            {
                Directory.CreateDirectory(walletPath);
            }
        }
    }
}
