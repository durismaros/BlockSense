using BlockSense.Models.Token;
using BlockSenseAPI.Models.Token;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlockSenseAPI.Services.TokenServices
{
    public interface IAccessTokenService
    {
        AccessTokenModel GenerateAccessToken(int userId);
    }

    public class AccessTokenService : IAccessTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseContext _dbContext;
        public AccessTokenService(IConfiguration configuration, DatabaseContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public AccessTokenModel GenerateAccessToken(int userId)
        {
            byte[] key = Convert.FromBase64String(_configuration["JwtConfig:Secret"]!);
            DateTime tokenExpiry = DateTime.UtcNow.AddMinutes(_configuration.GetValue<double>("JwtConfig:AccessTokenExpirationMinutes"));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Typ, "User")
                }),
                Expires = tokenExpiry,
                Issuer = _configuration["JwtConfig:Issuer"],
                Audience = _configuration["JwtConfig:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AccessTokenModel
            {
                Data = tokenHandler.WriteToken(token),
                ExpiresIn = (int)tokenExpiry.Subtract(DateTime.UtcNow).TotalSeconds
            };
        }
    }
}
