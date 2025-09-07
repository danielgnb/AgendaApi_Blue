using AgendaApi_Blue.Data;
using AgendaApi_Blue.Models;
using AgendaApi_Blue.Repositories.Interfaces;
using AgendaApi_Blue.Utilitaries;
using Microsoft.EntityFrameworkCore;

namespace AgendaApi_Blue.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;
        public UsuarioRepository(AppDbContext _context)
        {
            this._context = _context;
        }

        public async Task<Usuario?> ValidarUsuario(Usuario usuario)
        {
            string senhaHash = Utils.GerarHashSenha(usuario.Password);

            return await _context.Usuarios.Where(u => u.Username == usuario.Username
                                                && u.Password == senhaHash).FirstOrDefaultAsync();
        }

        public async Task<bool> CriarUsuario(Usuario usuario)
        {
            try
            {
                usuario.Password = Utils.GerarHashSenha(usuario.Password);

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> ExcluirUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Usuario?> ObterUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                return null;

            return usuario;
        }
    }
}
