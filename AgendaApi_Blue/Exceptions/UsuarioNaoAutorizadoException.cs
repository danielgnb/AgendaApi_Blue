namespace AgendaApi_Blue.Exceptions
{
    public class UsuarioNaoAutorizadoException : Exception
    {
        public UsuarioNaoAutorizadoException(string message) : base(message) { }
    }
}
