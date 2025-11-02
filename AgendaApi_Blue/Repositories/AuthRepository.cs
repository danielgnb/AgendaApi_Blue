using AgendaApi_Blue.Repositories.Interfaces;
using AgendaApi_Blue.Models;
using AgendaApi_Blue.Data;
using Microsoft.EntityFrameworkCore;

namespace AgendaApi_Blue.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;
        public AuthRepository(AppDbContext _context)
        {
            this._context = _context;
        }

        public Login? ObterPorRefreshToken(Login login)
        {
            try
            {
                var acesso = _context.Login
                    .Include(x => x.Usuario)
                    .Where(x => x.RefreshToken == login.RefreshToken)
                    .OrderByDescending(x => x.AccessDate)
                    .AsNoTracking()
                    .FirstOrDefault();

                return acesso;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task RegistrarAcesso(Login login)
        {
            try
            {
                _context.Login.Add(login);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}