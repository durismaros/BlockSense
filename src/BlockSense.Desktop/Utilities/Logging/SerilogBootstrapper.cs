using BlockSense.Desktop.Utilities.FileManagement;
using Serilog;
using Serilog.Events;

namespace BlockSense.Desktop.Utilities.Logging
{
    /// <summary>
    /// Provides centralized configuration and lifecycle management for Serilog logging within the BlockSense desktop application.
    /// </summary>
    public static class SerilogBootstrapper
    {
        /// <summary>
        /// Configures the global <see cref="Log"/> instance with application-specific settings.
        /// </summary>
        public static void Configure()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()

                // Reduce noise from framework internals
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Avalonia", LogEventLevel.Warning)

                .WriteTo.File(
                    path: DirectoryStructure.GetLogFilePath("blocksense.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    fileSizeLimitBytes: 10_485_760,
                    rollOnFileSizeLimit: true,
                    shared: true)

#if DEBUG
                // Output to console for development builds
                .WriteTo.Console(
                    outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                        "{Message:lj} " +
                        "{NewLine}{Exception}")
#endif

                .CreateLogger();
        }

        /// <summary>
        /// Shuts down Serilog and flushes any pending log events.
        /// Call this during application exit to ensure all logs are persisted.
        /// </summary>
        public static void Shutdown()
        {
            Log.CloseAndFlush();
        }
    }
}
