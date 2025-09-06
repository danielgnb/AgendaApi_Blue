using AgendaApi_Blue.Services.Interfaces;
using RabbitMQ.Client;
using System.Text;

public class RabbitMqService : IRabbitMqService
{
    private readonly IModel _channel;

    public RabbitMqService(IModel channel)
    {
        _channel = channel;
        _channel.ExchangeDeclare(exchange: "direct_logs", type: ExchangeType.Direct);
        _channel.QueueDeclare(queue: "log_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: "log_queue", exchange: "direct_logs", routingKey: "log");
    }

    public void EnviarMensagem(string mensagem)
    {
        var body = Encoding.UTF8.GetBytes(mensagem);

        _channel.BasicPublish(exchange: "direct_logs", routingKey: "log", basicProperties: null, body: body);
    }
}