using AgendaApi_Blue.Models;

namespace AgendaApi_Blue.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ValidarUsuario(Usuario usuario);
        Task<bool> CriarUsuario(Usuario usuario);
        Task<bool> ExcluirUsuario(int id);
        Task<Usuario?> ObterUsuario(int id);
    }
}
