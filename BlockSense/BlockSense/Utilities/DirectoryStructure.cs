using Avalonia.Controls.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Utilities
{
    class DirectoryStructure
    {
        public static string CoreDirectory { get; }
        public static string AuthPath { get; }
        public static string WalletPath { get; }
        public static string LogsPath { get; }

        static DirectoryStructure()
        {
            CoreDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BlockSense");
            AuthPath = Path.Combine(CoreDirectory, "auth");
            WalletPath = Path.Combine(CoreDirectory, "blocksense.wallet");
            LogsPath = Path.Combine(CoreDirectory, "logs");

            if (!Directory.Exists(CoreDirectory))
            {
                // Create and Mark base directory as hidden
                Directory.CreateDirectory(CoreDirectory);
                File.SetAttributes(CoreDirectory, FileAttributes.Hidden | FileAttributes.NotContentIndexed);
            }

            Directory.CreateDirectory(LogsPath);

            ConsoleHelper.Log("Storage structure initialized");
        }
    }
}
