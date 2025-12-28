using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Registration
{
    public sealed class InvalidInvitationCodeException : ApiException
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

        public override string ResultCode
        {
            get
            {
                return ResultCodes.Registration.InvalidInvitation;
            }
        }

        public InvalidInvitationCodeException()
            : base("Invalid or expired invitation code.") { }
    }
}
