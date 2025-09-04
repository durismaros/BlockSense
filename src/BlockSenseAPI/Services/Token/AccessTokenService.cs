using BlockSenseAPI.Models.Token;
using BlockSenseAPI.Models.Token.Configs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlockSenseAPI.Services.Token
{
    public class AccessTokenService : IAccessTokenService
    {
        private readonly AccessTokenConfig _config;
        public AccessTokenService(IOptions<AccessTokenConfig> jwtConfig)
        {
            _config = jwtConfig.Value;
        }

        public AccessToken GenerateAccessToken(int userId)
        {
            byte[] key = Convert.FromBase64String(_config.Secret);
            DateTime tokenExpiry = DateTime.UtcNow.AddMinutes(_config.AccessTokenExpirationMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Typ, "User")
                }),
                Expires = tokenExpiry,
                Issuer = _config.Issuer,
                Audience = _config.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AccessToken
            {
                Data = tokenHandler.WriteToken(token),
                ExpiresIn = (int)tokenExpiry.Subtract(DateTime.UtcNow).TotalSeconds
            };
        }
    }
}
