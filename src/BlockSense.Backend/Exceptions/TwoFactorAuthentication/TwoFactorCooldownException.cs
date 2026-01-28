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
            : base($"Backup codes cannot be generated yet. Please wait {remainingTime} before trying again.") { }
    }
}
