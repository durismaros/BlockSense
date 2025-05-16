namespace BlockSenseAPI.Models.Requests
{
    public class LoginRequestModel
    {
        public string? Login { get; set; }
        public string? Password { get; set; }
        public SystemIdentifierModel? Identifiers { get; set; }
    }
}
