namespace BlockSense.Backend.Exceptions.Registration
{
    public sealed class InvalidInvitationCodeException : AppException
    {
        public InvalidInvitationCodeException()
            : base("Invalid or expired invitation code.") { }
    }
}
