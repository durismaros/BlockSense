namespace BlockSense.Backend.Exceptions.Authentication
{
    public abstract class AuthenticationException : AppException
    {
        protected AuthenticationException(string message)
            : base(message) { }
    }
}
