using AgendaApi_Blue.Models;

namespace AgendaApi_Blue.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Login? ObterPorRefreshToken(Login login);
        Task RegistrarAcesso(Login login);
    }
}