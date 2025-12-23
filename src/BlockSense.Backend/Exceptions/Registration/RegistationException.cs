namespace BlockSense.Backend.Exceptions.Registration
{
    public abstract class RegistationException : AppException
    {
        protected RegistationException(string message)
            : base(message) { }
    }
}
