using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Registration
{
    public sealed class UsernameTakenException : RegistationException
    {
        public override int Status
        {
            get
            {
                return StatusCodes.Status409Conflict;
            }
        }
        public override string Title
        {
            get
            {
                return "Duplicate resource";
            }
        }
        public override string ErrorCode
        {
            get
            {
                return ErrorCodes.Registration.UsernameTaken;
            }
        }

        public UsernameTakenException()
            : base("Username already in use.") { }
    }
}
