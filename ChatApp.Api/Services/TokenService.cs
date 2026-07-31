using ChatApp.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatApp.Api.Services
{
    interface ITokenService
    {
        (string AccessToken, DateTime Expiration) GenerateAccessToken(User user);
    }

    public class TokenService : ITokenService
    {
        public readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }   

        public (string AccessToken, DateTime Expiration) GenerateAccessToken(User user) {
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var exp = DateTime.UtcNow.AddMinutes(30);
            var authSigninKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]!));
            var creds = new SigningCredentials(authSigninKey, SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(_configuration["JWT:ValidIssuer"],
                                             _configuration["JWT:ValidAudience"],
                                             authClaims,
                                             expires: exp,
                                             signingCredentials: creds);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, exp);
        }
    }
}
