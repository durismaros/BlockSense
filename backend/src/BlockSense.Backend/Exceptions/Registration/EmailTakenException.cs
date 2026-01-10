using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Registration
{
    public sealed class EmailTakenException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.Registration.EmailTaken;
            }
        }
        public override string Title
        {
            get
            {
                return "Email Already Registered";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status409Conflict;
            }
        }

        public EmailTakenException()
            : base("The email address you entered is already associated with an existing account. Please use a different email or sign in if you already have an account.") { }
    }
}
