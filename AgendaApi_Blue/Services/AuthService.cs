using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using AgendaApi_Blue.Services.Interfaces;
using System.Security.Claims;
using System.Text;
using AgendaApi_Blue.Models;

namespace AgendaApi_Blue.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GerarToken(string username, int idUsuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("UsuarioId", idUsuario.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}