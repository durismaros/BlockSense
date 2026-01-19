using System;

namespace BlockSense.Desktop.Utilities.ApiHandling
{
    public sealed class AuthenticationRequiredException : Exception
    {
        public AuthenticationRequiredException()
            : base("Authentication is required.") { }
    }
}
