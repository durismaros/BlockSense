namespace BlockSense.Backend.Exceptions.Generic
{
    public class CustomException : ApiException
    {
        public override string Type
        {
            get;
        }

        public override string Title
        {
            get;
        }

        public override int Status
        {
            get;
        }

        public CustomException(string type, string title, int status, string message) : base(message)
        {
            this.Type = type;
            this.Title = title;
            this.Status = status;
        }
    }
}
