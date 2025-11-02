using System.Security.Cryptography;
using System.Text;

namespace AgendaApi_Blue.Utilitaries
{
    public static class Utils
    {
        public static string GerarHashSenha(string senha)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));

            return Convert.ToBase64String(bytes);
        }

        public static string GerarHashToken(string token)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));

            return Convert.ToBase64String(bytes);
        }
    }
}