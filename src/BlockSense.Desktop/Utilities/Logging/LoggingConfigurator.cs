using BlockSense.Desktop.Utilities.FileManagement;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System;
using System.IO;

namespace BlockSense.Desktop.Utilities.Logging
{
    public static class LoggingConfigurator
    {
        public static void Configure()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()

                // Reduce noise from framework internals
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Avalonia", LogEventLevel.Warning)

                // Enrichment
                .Enrich.FromLogContext()
                
                #if DEBUG
                .WriteTo.Console(
                    outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                        "[{SourceContext}] " +
                        "{Message:lj} " +
                        "{NewLine}{Exception}")
                #endif

                // Rolling file (JSON)
                .WriteTo.Async(a => a.File(
                    new JsonFormatter(),
                    path: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "BlockSense", "logs", "blocksense-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    fileSizeLimitBytes: 10_000_000,
                    rollOnFileSizeLimit: true,
                    shared: true
                ))

                .CreateLogger();
        }
    }
}
