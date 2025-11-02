using System.ComponentModel.DataAnnotations.Schema;

namespace AgendaApi_Blue.Models
{
    public class Login
    {
        public Login()
        {
            
        }

        public int Id { get; set; }
        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiration { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
        public DateTime AccessDate { get; set; }
        public Usuario Usuario { get; set; }
    }
}