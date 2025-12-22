namespace BlockSense.Backend.Exceptions.Registration
{
    public sealed class UsernameTakenException : AppException
    {
        public UsernameTakenException()
            : base("Username already in use.") { }
    }
}
