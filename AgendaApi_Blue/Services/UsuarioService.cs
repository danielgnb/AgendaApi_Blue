using AgendaApi_Blue.Repositories.Interfaces;
using AgendaApi_Blue.Services.Interfaces;
using AgendaApi_Blue.Utilitaries;
using AgendaApi_Blue.Exceptions;
using AgendaApi_Blue.Models;
using System.Security.Claims;

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
            usuario.Password = Utils.GerarHashSenha(usuario.Password);

            return _usuarioRepository.ValidarUsuario(usuario);
        }

        public Task<bool> CriarUsuario(Usuario usuario)
        {
            usuario.Password = Utils.GerarHashSenha(usuario.Password);

            return _usuarioRepository.CriarUsuario(usuario);
        }

        public Task<bool> ExcluirUsuario(int id)
        {
            return _usuarioRepository.ExcluirUsuario(id);
        }

        public Task<Usuario?> ObterUsuario(int id)
        {
            return _usuarioRepository.ObterUsuario(id);
        }

        public async Task<bool> EditarUsuario(Usuario usuario, int id)
        {
            var user = await _usuarioRepository.ObterUsuario(id);
            if (user == null)
                throw new UsuarioNaoEncontradoException("Usuário não encontrado.");

            user.Username = usuario.Username;
            user.Password = Utils.GerarHashSenha(usuario.Password);
            user.Role = usuario.Role;

            return await _usuarioRepository.EditarUsuario(user);
        }

        public async Task ValidarEditar(int usuarioLogado, string roleLogada, int id)
        {
            if (roleLogada == nameof(Enums.Role.Admin))
                return;

            var usuarioAtual = await ObterUsuario(id);
            if (usuarioAtual == null)
                throw new UsuarioNaoEncontradoException("Usuário não encontrado.");

            if (usuarioAtual.Id != usuarioLogado)
                throw new UsuarioNaoAutorizadoException("Você não tem permissão para editar este usuário.");
        }
    }
}