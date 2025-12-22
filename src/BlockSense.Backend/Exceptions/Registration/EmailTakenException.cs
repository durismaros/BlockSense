namespace BlockSense.Backend.Exceptions.Registration
{
    public sealed class EmailTakenException : AppException
    {
        public EmailTakenException()
            : base("Email already in use.") { }
    }
}
