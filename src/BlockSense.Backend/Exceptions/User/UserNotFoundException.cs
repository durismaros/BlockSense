using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.User
{
    public sealed class UserNotFoundException : ApiException
    {
        public override string Type
        {
            get
            {
                return ApiProblemTypes.Generic.BadRequest;
            }
        }

        public override string Title
        {
            get
            {
                return "User not Found";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status404NotFound;
            }
        }

        public UserNotFoundException()
            : base("The requested user could not be found.") { }
    }
}
