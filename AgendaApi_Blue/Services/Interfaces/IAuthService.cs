using AgendaApi_Blue.Utilitaries;

namespace AgendaApi_Blue.Services.Interfaces
{
    public interface IAuthService
    {
        string GerarToken(string userName, int idUsuario, Enums.Role role);
    }
}
