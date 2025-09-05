using AgendaApi_Blue.Models;
using AgendaApi_Blue.Repositories.Interfaces;
using AgendaApi_Blue.Services.Interfaces;

namespace AgendaApi_Blue.Services
{
    public class ContatoService : IContatoService
    {
        private readonly IContatoRepository _contatoRepository;

        public ContatoService(IContatoRepository _contatoRepository)
        {
            this._contatoRepository = _contatoRepository;
        }

        public Task<bool> CriarContato(Contato contato)
        {
            return _contatoRepository.CriarContato(contato);
        }

        public Task<bool> EditarContato(Contato contato)
        {
            return _contatoRepository.EditarContato(contato);
        }

        public Task<Contato?> ObterContato(int Id)
        {
            return _contatoRepository.ObterContato(Id);
        }

        public Task<List<Contato?>> ObterContatos()
        {
            return _contatoRepository.ObterContatos();
        }

        public Task<List<Contato?>> ObterContatosPorUsuario(int idUsuario)
        {
            return _contatoRepository.ObterContatosPorUsuario(idUsuario);
        }

        public Task<bool> RemoverContato(int Id)
        {
            return _contatoRepository.RemoverContato(Id);
        }
    }
}