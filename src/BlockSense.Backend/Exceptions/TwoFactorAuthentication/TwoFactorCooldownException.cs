using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.TwoFactorAuthentication
{
    public class TwoFactorCooldownException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.TwoFactorAuthentication.BackupCodesCooldown;
            }
        }

        public override string Title
        {
            get
            {
                return "Backup Codes Cooldown";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status429TooManyRequests;
            }
        }

        public TwoFactorCooldownException(TimeSpan remainingTime)
            : base($"Backup codes cannot be generated yet. Please wait {FormatTime(remainingTime)} before trying again.") { }

        private static string FormatTime(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                int hours = (int)timeSpan.TotalHours;
                return hours == 1 ? "1 hour" : $"{hours} hours";
            }
            else
            {
                int minutes = (int)timeSpan.TotalMinutes;
                return minutes == 1 ? "1 minute" : $"{minutes} minutes";
            }
        }
    }
}
