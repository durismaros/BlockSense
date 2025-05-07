using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using BlockSense.Server;
using BlockSense.DatabaseUtils;
using System.Data;
using System.Diagnostics;
using ReactiveUI;

namespace BlockSense.Client.Utilities
{
    class ActivityLogger
    {
        private const string LogFileName = "activity_logs.json";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        private readonly static string _logFilePath = Path.Combine(DirStructure.logsPath, LogFileName);

        public static void InitializeApplicationLogger()
        {
            if (!File.Exists(_logFilePath))
            {
                ActivityLogFile newActivityLogFile = new()
                {
                    FileMetadata = new LogMetadata
                    {
                        Version = 1.0f,
                        LastUpdated = DateTime.UtcNow,
                        Application = "BlockSense"
                    },
                    Entries = new List<ActivityLog>()
                };
                SaveLogs(newActivityLogFile);
            }
            else if (File.Exists(_logFilePath))
            {
                GetDeviceInfo();
            }

            ConsoleHelper.Log("Activity logs initialized");
        }

        private static async void GetDeviceInfo()
        {
            ActivityLogFile currentLogs = LoadExistingLogs();
            // Remove existing device logs
            currentLogs.Entries.RemoveAll(log => log.Activity == ActivityType.DeviceLogin);

            string query = "select hardware_identifier as hwid, min(issued_at) as first_login, max(if(revoked = 0 and expires_at > NOW(), 1, 0)) as active from refreshtokens where user_id = @user_id group by hardware_identifier";
            Dictionary<string, object> parameters = new()
            {
                { "user_id", User.Uid },
            };
            using (var reader = await Database.FetchData(query, parameters))
            {
                while (reader.Read())
                {
                    ActivityStatus status = (reader.GetBoolean("active")) ? ActivityStatus.Active : ActivityStatus.Inactive;

                    ActivityLog deviceLog = new()
                    {
                        Timestamp = reader.GetDateTime("first_login"),
                        Activity = ActivityType.DeviceLogin,
                        Status = status,
                        Metadata = new()
                        {
                            {"hardware_identifier", reader.GetString("hwid")}
                        }
                    };

                    LogActivity(deviceLog);
                }
            }
        }

        public static void LogActivity(ActivityLog logEntry)
        {
            ActivityLogFile existingLogs = LoadExistingLogs();
            if (existingLogs.Entries.Count == 0 || logEntry.Timestamp >= existingLogs.Entries.Last().Timestamp)
            {
                existingLogs.Entries.Add(logEntry);
            }
            else
            {
                existingLogs.Entries.Add(logEntry);
                existingLogs.Entries.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
            }
            SaveLogs(existingLogs);
        }

        public static ActivityLogFile LoadExistingLogs()
        {
            try
            {
                var json = File.ReadAllText(_logFilePath);
                return JsonSerializer.Deserialize<ActivityLogFile>(json, JsonOptions) ?? new ActivityLogFile();
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);

                // If file is corrupted, start fresh
                return new ActivityLogFile();
            }
        }

        private static void SaveLogs(ActivityLogFile logFile)
        {
            logFile.FileMetadata.LastUpdated = DateTime.UtcNow;
            try
            {
                var tempPath = $"{_logFilePath}.tmp";
                var json = JsonSerializer.Serialize(logFile, JsonOptions);
                File.WriteAllText(tempPath, json);
                if (!File.Exists(_logFilePath))
                    File.Move(tempPath, _logFilePath);
                else
                    File.Replace(tempPath, _logFilePath, null);
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log("Error: " + ex.Message);
            }
        }

        public class ActivityLogFile
        {
            public LogMetadata FileMetadata { get; set; } = new();
            public List<ActivityLog> Entries { get; set; } = new();
        }

        public class LogMetadata
        {
            public float Version { get; set; }
            public DateTime LastUpdated { get; set; }
            public string Application { get; set; } = string.Empty;
        }

        public class ActivityLog
        {
            public DateTime Timestamp { get; set; }
            public ActivityType Activity { get; set; }
            public ActivityStatus Status { get; set; }
            public Dictionary<string, string> Metadata { get; set; } = new();
        }

        public enum ActivityType
        {
            AccountCreated,
            DeviceLogin,
            PasswordChange,
            TwoFaEnabled,
            TwoFaDisabled,
            WalletCreated,
            WalletImported,
            BackupCreated
        }

        public enum ActivityStatus
        {
            Completed,
            Verified,
            Failed,
            Active,
            Inactive,
            Pending
        }

    }
}
