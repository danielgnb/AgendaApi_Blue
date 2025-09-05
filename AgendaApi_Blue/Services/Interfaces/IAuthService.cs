namespace AgendaApi_Blue.Services.Interfaces
{
    public interface IAuthService
    {
        string GerarToken(string userName, int idUsuario);
    }
}
