using BlockSense.Contracts.Definitions;

namespace BlockSense.Backend.Exceptions.Generic
{
    public sealed class NotFoundException : ApiException
    {
        public override string Type
        {
            get
            {
                return StandardizedCodes.Generic.NotFound;
            }
        }

        public override string Title
        {
            get
            {
                return "Not Found";
            }
        }

        public override int Status
        {
            get
            {
                return StatusCodes.Status404NotFound;
            }
        }

        public NotFoundException()
            : base("The requested resource does not exist or is no longer available.") { }
    }
}
