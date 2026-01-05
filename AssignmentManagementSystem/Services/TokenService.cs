using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AssignmentManagementSystem.Models;
using Microsoft.IdentityModel.Tokens;

namespace AssignmentManagementSystem.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public string GenerateToken(ApplicationUser user, IList<string> roles)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
