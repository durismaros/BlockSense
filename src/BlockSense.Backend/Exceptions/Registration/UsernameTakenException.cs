using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Registration
{
    public sealed class UsernameTakenException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.Registration.UsernameTaken;
            }
        }

        public override string Title
        {
            get
            {
                return "Username Already Taken";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status409Conflict;
            }
        }

        public UsernameTakenException()
            : base("The username you selected is not available. Please choose a different username.") { }
    }
}
