namespace BlockSense.Desktop.Models
{
    public sealed record ServiceResponse
    {
        public required string ProblemType
        {
            get;
            init;
        }

        public required string Message
        {
            get;
            init;
        }
    }
}
