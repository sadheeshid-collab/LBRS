using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LBRS.Member.Service.Helper
{
    public class JwtAuthTokenService: IJwtAuthTokenService
    {
        private readonly JwtSettings _jwt;

        public JwtAuthTokenService(IOptions<JwtSettings> jwt)
        {
            _jwt = jwt.Value;
        }

        public Task<string> GenerateAccessToken(string userRoleType, Guid userID)
        {

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userID.ToString()),
                new Claim(ClaimTypes.Role, userRoleType)
            };

  
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwt.Key));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            var tokenDescriptor = new JwtSecurityToken
            (
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
                signingCredentials: creds
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(tokenDescriptor));

        }



    }
}
