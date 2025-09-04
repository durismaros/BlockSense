namespace BlockSenseAPI.Models.Login
{
    public class LoginRequest
    {
        public string? Login { get; set; }
        public string? Password { get; set; }
        public SystemIdentifier? Identifiers { get; set; }
    }
}
