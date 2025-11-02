using AgendaApi_Blue.Models.DTOs.Auth;
using AgendaApi_Blue.Utilitaries;
using AgendaApi_Blue.Models;

namespace AgendaApi_Blue.Services.Interfaces
{
    public interface IAuthService
    {
        (string accessToken, string refreshToken) GerarToken(string userName, int idUsuario, Enums.Role role);
        Login? ObterPorRefreshToken(string refreshToken);
        Task RegistrarAcesso(AcessoDTO acessoDTO);
    }
}
