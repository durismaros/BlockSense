using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Registration
{
    public sealed class InvalidInvitationCodeException : RegistationException
    {
        public override int Status
        {
            get
            {
                return StatusCodes.Status401Unauthorized;
            }
        }

        public override string Title
        {
            get
            {
                return "Invalid invitation code";
            }
        }

        public override string ErrorCode
        {
            get
            {
                return ErrorCodes.Registration.InvalidInvitation;
            }
        }

        public InvalidInvitationCodeException()
            : base("Invalid or expired invitation code.") { }
    }
}
