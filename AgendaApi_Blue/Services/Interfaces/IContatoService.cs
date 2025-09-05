using AgendaApi_Blue.Models;

namespace AgendaApi_Blue.Services.Interfaces
{
    public interface IContatoService
    {
        Task<List<Contato?>> ObterContatos();
        Task<Contato?> ObterContato(int Id);
        Task<bool> RemoverContato(int Id);
        Task<bool> CriarContato(Contato contato);
        Task<bool> EditarContato(Contato contato);
        Task<List<Contato?>> ObterContatosPorUsuario(int idUsuario);
    }
}