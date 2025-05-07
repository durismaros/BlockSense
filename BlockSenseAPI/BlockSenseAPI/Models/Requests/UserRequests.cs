namespace BlockSenseAPI.Models.Requests
{
    // Request models
    public class LoginRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public SystemIdentifiers Identifiers { get; set; }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string InvitationCode { get; set; }
    }
}
