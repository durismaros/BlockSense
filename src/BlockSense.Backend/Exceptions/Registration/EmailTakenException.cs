using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Registration
{
    public sealed class EmailTakenException : ApiException
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
                return ResultCodes.Registration.EmailTaken;
            }
        }

        public EmailTakenException()
            : base("Email already in use.") { }
    }
}
