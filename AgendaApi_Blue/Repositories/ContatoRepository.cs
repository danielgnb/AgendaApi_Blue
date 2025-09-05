using AgendaApi_Blue.Data;
using AgendaApi_Blue.Models;
using AgendaApi_Blue.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaApi_Blue.Repositories
{
    public class ContatoRepository : IContatoRepository
    {
        private readonly AppDbContext _context;
        public ContatoRepository(AppDbContext _context)
        {
            this._context = _context;
        }

        public async Task<bool> CriarContato(Contato contato)
        {
            try
            {
                _context.Add(contato);
                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> EditarContato(Contato contato)
        {
            try
            {
                var contatoExistente = await _context.Contatos
                     .FirstOrDefaultAsync(c => c.Id == contato.Id && c.IdUsuario == contato.IdUsuario);

                if (contatoExistente == null)
                    return false;

                _context.Entry(contatoExistente).CurrentValues.SetValues(contato);
                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Contato?> ObterContato(int id)
        {
            try
            {
                var contato = await _context.Contatos.FindAsync(id);

                return contato;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Contato?>> ObterContatos()
        {
            try
            {
                return await _context.Contatos.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Contato?>> ObterContatosPorUsuario(int idUsuario)
        {
            try
            {
                var contatos = await _context.Contatos.Where(x => x.IdUsuario == idUsuario).ToListAsync();

                return contatos;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoverContato(int id)
        {
            try
            {
                var contato = await _context.Contatos.FindAsync(id);
                if (contato == null)
                    return false;

                _context.Contatos.Remove(contato);

                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}