namespace AgendaApi_Blue.Services.Interfaces
{
    public interface IRabbitMqService
    {
        void EnviarMensagem(string mensagem);
    }
}
