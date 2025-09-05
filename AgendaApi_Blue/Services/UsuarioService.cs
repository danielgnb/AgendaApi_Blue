using AgendaApi_Blue.Models;
using AgendaApi_Blue.Repositories.Interfaces;
using AgendaApi_Blue.Services.Interfaces;

namespace AgendaApi_Blue.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository _usuarioRepository)
        {
            this._usuarioRepository = _usuarioRepository;
        }

        public Task<Usuario?> ValidarUsuario(Usuario usuario)
        {
            return _usuarioRepository.ValidarUsuario(usuario);
        }

        public Task<bool> CriarUsuario(Usuario usuario)
        {
            return _usuarioRepository.CriarUsuario(usuario);
        }

        public Task<bool> ExcluirUsuario(int id)
        {
            return _usuarioRepository.ExcluirUsuario(id);
        }
    }
}
