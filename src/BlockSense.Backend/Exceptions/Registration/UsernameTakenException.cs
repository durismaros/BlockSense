using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Registration
{
    public sealed class UsernameTakenException : ApiException
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
        public override string ResultCode
        {
            get
            {
                return ResultCodes.Registration.UsernameTaken;
            }
        }

        public UsernameTakenException()
            : base("Username already in use.") { }
    }
}
