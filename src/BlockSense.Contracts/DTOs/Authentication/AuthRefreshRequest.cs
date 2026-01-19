using System.ComponentModel.DataAnnotations;

namespace BlockSense.Contracts.DTOs.Authentication
{
    public sealed record AuthRefreshRequest
    {
        [Required(ErrorMessage = "Refresh Token is required.")]
        public required string RefreshToken
        {
            get;
            init;
        }
    }
}
