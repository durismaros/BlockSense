using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace BlockSense.Desktop.Utilities.FileManagement
{
    /// <summary>
    /// Provides application-specific directory paths and ensures the required directory structure exists.
    /// </summary>
    public sealed class DirectoryStructure
    {
        private readonly ILogger<DirectoryStructure> _logger;

        /// <summary>
        /// Base directory for BlockSense application data.
        /// </summary>
        public string AppDataDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Directory for authentication-related files such as tokens and user metadata.
        /// </summary>
        public string AuthDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Full path to the wallet file where blockchain-related data is stored.
        /// </summary>
        public string WalletDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Directory for application log files.
        /// </summary>
        public string LogsDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor initializes directory paths and ensures the required folder structure exists.
        /// </summary>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DirectoryStructure(ILogger<DirectoryStructure> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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

                _logger.LogInformation(
                    "Directory structure initialized successfully at `{AppDataDirectory}`",
                    AppDataDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "Failed to initialize directory structure at `{AppDataDirectory}`",
                    AppDataDirectory);

                throw;
            }
        }

        /// <summary>
        /// Ensures the specified directory exists and optionally sets it as hidden.
        /// </summary>
        /// <param name="path">The directory path to ensure exists.</param>
        /// <param name="hidden">Whether to mark the directory as hidden.</param>
        private void EnsureDirectory(string path, bool hidden = false)
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
                    _logger.LogWarning(
                        ex,
                        "Failed to apply hidden attributes to directory `{Path}`",
                        path);
                }
            }
        }

        /// <summary>
        /// Returns a path to a file inside the wallet directory for the current profile.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string GetWalletFilePath(string filename) => Path.Combine(WalletDirectory, filename);

        /// <summary>
        /// Returns a path to a file inside the auth directory.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string GetAuthFilePath(string filename) => Path.Combine(AuthDirectory, filename);

        /// <summary>
        /// Returns a path to a log file inside the logs directory.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string GetLogFilePath(string filename) => Path.Combine(LogsDirectory, filename);
    }
}
