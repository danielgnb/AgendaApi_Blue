namespace AgendaApi_Blue.Exceptions
{
    public class UsuarioNaoEncontradoException : Exception
    {
        public UsuarioNaoEncontradoException(string message) : base(message) { }
    }
}