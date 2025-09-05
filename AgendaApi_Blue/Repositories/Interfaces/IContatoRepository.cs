using AgendaApi_Blue.Models;

namespace AgendaApi_Blue.Repositories.Interfaces
{
    public interface IContatoRepository
    {
        Task<bool> CriarContato(Contato contato);
        Task<bool> EditarContato(Contato contato);
        Task<Contato?> ObterContato(int id);
        Task<List<Contato?>> ObterContatos();
        Task<List<Contato?>> ObterContatosPorUsuario(int idUsuario);
        Task<bool> RemoverContato(int id);
    }
}
