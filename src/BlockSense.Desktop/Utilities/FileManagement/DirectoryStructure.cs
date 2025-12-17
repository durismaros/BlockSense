using BlockSense.Desktop.Utilities.Logging;
using System;
using System.IO;

namespace BlockSense.Desktop.Utilities.FileManagement
{
    /// <summary>
    /// Provides application-specific directory paths and ensures the required directory structure exists.
    /// </summary>
    public static class DirectoryStructure
    {
        /// <summary>
        /// Base directory for BlockSense application data.
        /// </summary>
        public static readonly string AppDataDirectory;

        /// <summary>
        /// Directory for authentication-related files such as tokens and user metadata.
        /// </summary>
        public static readonly string AuthDirectory;

        /// <summary>
        /// Full path to the wallet file where blockchain-related data is stored.
        /// </summary>
        public static readonly string WalletDirectory;

        /// <summary>
        /// Directory for application log files.
        /// </summary>
        public static readonly string LogsDirectory;

        /// <summary>
        /// Static constructor initializes directory paths and ensures the required folder structure exists.
        /// </summary>
        static DirectoryStructure()
        {
            AppDataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BlockSense");

            AuthDirectory = Path.Combine(AppDataDirectory, "auth");
            WalletDirectory = Path.Combine(AppDataDirectory, "blocksense.wallet");
            LogsDirectory = Path.Combine(AppDataDirectory, "logs");

            try
            {
                EnsureDirectory(AppDataDirectory, hidden: true);
                EnsureDirectory(AuthDirectory);
                EnsureDirectory(LogsDirectory);

                ConsoleLogger.Log("Storage structure initialized successfully");
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log($"Error initializing storage structure: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures the specified directory exists and optionally sets it as hidden.
        /// </summary>
        /// <param name="path">The directory path to ensure exists.</param>
        /// <param name="hidden">Whether to mark the directory as hidden.</param>
        private static void EnsureDirectory(string path, bool hidden = false)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (hidden)
            {
                try
                {
                    var attributes = File.GetAttributes(path);
                    File.SetAttributes(path, attributes | FileAttributes.Hidden | FileAttributes.NotContentIndexed);
                }
                catch (Exception ex)
                {
                    ConsoleLogger.Log($"Failed to set hidden attribute for {path}: {ex.Message}");
                }
            }
        }
    }
}
