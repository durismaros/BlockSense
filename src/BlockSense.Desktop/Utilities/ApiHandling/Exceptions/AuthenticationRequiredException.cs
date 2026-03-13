using System;

namespace BlockSense.Desktop.Utilities.ApiHandling.Exceptions
{
    public sealed class AuthenticationRequiredException : Exception
    {
        public AuthenticationRequiredException()
            : base("Authentication is required. Please sign in again.") { }

        public AuthenticationRequiredException(string message)
            : base(message) { }

        public AuthenticationRequiredException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
