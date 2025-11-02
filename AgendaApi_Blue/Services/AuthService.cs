using AgendaApi_Blue.Repositories.Interfaces;
using AgendaApi_Blue.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using AgendaApi_Blue.Models.DTOs.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using AgendaApi_Blue.Utilitaries;
using System.Security.Claims;
using AgendaApi_Blue.Models;
using System.Text;

namespace AgendaApi_Blue.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public AuthService(IConfiguration configuration, IWebHostEnvironment environment, IAuthRepository authRepository)
        {
            _configuration = configuration;
            _environment = environment;
            _authRepository = authRepository;
        }

        public (string accessToken, string refreshToken) GerarToken(string username, int idUsuario, Enums.Role role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("UsuarioId", idUsuario.ToString()),
                new Claim(ClaimTypes.Role, role.ToString())
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

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = RefreshToken();

            return (accessToken, refreshToken);
        }

        public Login? ObterPorRefreshToken(string refreshToken)
        {
            var login = new Login();

            login.RefreshToken = _environment.IsProduction()
                ? Utils.GerarHashToken(refreshToken)
                : refreshToken;

            return _authRepository.ObterPorRefreshToken(login);
        }

        public async Task RegistrarAcesso(AcessoDTO acessoDTO)
        {
            var login = new Login();

            login.IdUsuario = acessoDTO.IdUsuario;
            login.AccessToken = _environment.IsProduction()
                ? Utils.GerarHashToken(acessoDTO.AccessToken)
                : acessoDTO.AccessToken;
            login.AccessTokenExpiration = DateTime.Now.AddHours(1);
            login.RefreshToken = _environment.IsProduction()
                ? Utils.GerarHashToken(acessoDTO.RefreshToken)
                : acessoDTO.RefreshToken;
            login.RefreshTokenExpiration = DateTime.Now.AddDays(7);
            login.AccessDate = DateTime.Now;

            await _authRepository.RegistrarAcesso(login);
        }

        private static string RefreshToken()
        {
            var randomBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            var refreshToken = Convert.ToBase64String(randomBytes);

            return refreshToken;
        }
    }
}