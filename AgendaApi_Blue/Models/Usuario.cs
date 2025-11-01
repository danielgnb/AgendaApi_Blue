using static AgendaApi_Blue.Utilitaries.Enums;

namespace AgendaApi_Blue.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }
        public ICollection<Contato> Contatos { get; set; } = new List<Contato>();
    }
}