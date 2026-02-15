using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Registration
{
    public sealed class InvalidInvitationCodeException : ApiException
    {
        public override string Type
        {
            get
            {
                return StandardizedCodes.Registration.InvalidInvitation;
            }
        }

        public override string Title
        {
            get
            {
                return "Invalid Invitation Code";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status401Unauthorized;
            }
        }

        public InvalidInvitationCodeException()
            : base("The invitation code you entered is not valid. Please check the code and try again, or contact support if you believe this is an error.") { }
    }
}
