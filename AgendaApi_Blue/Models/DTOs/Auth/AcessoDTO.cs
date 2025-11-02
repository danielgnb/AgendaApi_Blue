namespace AgendaApi_Blue.Models.DTOs.Auth
{
    public class AcessoDTO
    {
        public int IdUsuario { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
