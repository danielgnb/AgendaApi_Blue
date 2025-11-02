using AgendaApi_Blue.Models;

namespace AgendaApi_Blue.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<Usuario?> ValidarUsuario(Usuario usuario);
        Task<bool> CriarUsuario(Usuario usuario);
        Task<bool> ExcluirUsuario(int id);
        Task<Usuario?> ObterUsuario(int id);
        Task<bool> EditarUsuario(Usuario usuario, int id);
    }
}