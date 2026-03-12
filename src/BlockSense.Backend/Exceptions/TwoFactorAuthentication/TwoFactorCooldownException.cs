using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.TwoFactorAuthentication
{
    /// <summary>
    /// Thrown when backup codes cannot be regenerated because the cooldown period has not yet elapsed.
    /// </summary>
    public sealed class TwoFactorCooldownException : ApiException
    {
        /// <inheritdoc/>
        public override string Type => StandardizedCodes.TwoFactorAuthentication.BackupCodesCooldown;

        /// <inheritdoc/>
        public override string Title => "Backup Codes Cooldown";

        /// <inheritdoc/>
        public override int Status => StatusCodes.Status429TooManyRequests;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoFactorCooldownException"/> class.
        /// </summary>
        /// <param name="remainingTime">The time remaining before backup codes can be regenerated.</param>
        public TwoFactorCooldownException(TimeSpan remainingTime)
            : base($"Backup codes cannot be generated yet. Please wait {FormatRemainingTime(remainingTime)} before trying again.") { }

        /// <summary>
        /// Formats a <see cref="TimeSpan"/> into a human-readable duration string (e.g., "1 hour", "5 minutes").
        /// </summary>
        /// <param name="remainingTime">The duration to format.</param>
        /// <returns>A human-readable string representing the duration in hours or minutes.</returns>
        private static string FormatRemainingTime(TimeSpan remainingTime)
        {
            if (remainingTime.TotalHours >= 1)
            {
                int hours = (int)remainingTime.TotalHours;
                return hours == 1 ? "1 hour" : $"{hours} hours";
            }

            int minutes = (int)remainingTime.TotalMinutes;
            return minutes == 1 ? "1 minute" : $"{minutes} minutes";
        }
    }
}