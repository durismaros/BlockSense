using Serilog;
using System;
using System.IO;

namespace BlockSense.Desktop.Utilities.FileManagement
{
    /// <summary>
    /// Provides centralized access to application-specific directory paths and ensures the required directory structure exists at startup.
    /// </summary>
    public static class DirectoryStructure
    {
        /// <summary>
        /// Base directory for all BlockSense application data.
        /// Located under the user's application data folder.
        /// </summary>
        public static string AppDataDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Directory for authentication-related data such as encrypted tokens and user session metadata.
        /// </summary>
        public static string AuthDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Directory for wallet-related files containing blockchain and key data.
        /// </summary>
        public static string WalletDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Directory for application log files.
        /// </summary>
        public static string LogsDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the directory structure and ensures all required folders exist.
        /// </summary>
        static DirectoryStructure()
        {
            AppDataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BlockSense");

            AuthDirectory = Path.Combine(
                AppDataDirectory,
                "auth");

            WalletDirectory = Path.Combine(
                AppDataDirectory,
                "wallet");

            LogsDirectory = Path.Combine(
                AppDataDirectory,
                "logs");

            try
            {
                EnsureDirectory(AppDataDirectory, hidden: true);
                EnsureDirectory(AuthDirectory);
                EnsureDirectory(WalletDirectory);
                EnsureDirectory(LogsDirectory);

                Log.Information(
                    "Directory structure initialized successfully at `{AppDataDirectory}`",
                    AppDataDirectory);
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Failed to initialize directory structure at `{AppDataDirectory}`",
                    AppDataDirectory);

                throw;
            }
        }


        /// <summary>
        /// Ensures that a directory exists at the specified path and optionally applies hidden and non-indexed attributes on supported platforms.
        /// </summary>
        /// <param name="path">The directory path to create or validate.</param>
        /// <param name="hidden">Whether to mark the directory as hidden (Windows only).</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="path"/> is <c>null</c>.</exception>
        private static void EnsureDirectory(string path, bool hidden = false)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (hidden && OperatingSystem.IsWindows())
            {
                try
                {
                    var fileAttributes = File.GetAttributes(path) | FileAttributes.Hidden | FileAttributes.NotContentIndexed;
                    File.SetAttributes(path, fileAttributes);
                }
                catch (Exception ex)
                {
                    Log.Warning(
                        ex,
                        "Failed to apply hidden attributes to directory `{Path}`",
                        path);
                }
            }
        }

        /// <summary>
        /// Returns the full path to a file located within the wallet directory.
        /// </summary>
        /// <param name="filename">The wallet file name.</param>
        /// <returns>The combined wallet file path.</returns>
        public static string GetWalletFilePath(string filename)
            => Path.Combine(WalletDirectory, filename);

        /// <summary>
        /// Returns the full path to a file located within the authentication directory.
        /// </summary>
        /// <param name="filename">The authentication file name.</param>
        /// <returns>The combined authentication file path.</returns>
        public static string GetAuthFilePath(string filename)
            => Path.Combine(AuthDirectory, filename);

        /// <summary>
        /// Returns the full path to a file located within the logs directory.
        /// </summary>
        /// <param name="filename">The log file name.</param>
        /// <returns>The combined log file path.</returns>
        public static string GetLogFilePath(string filename)
            => Path.Combine(LogsDirectory, filename);
    }
}
