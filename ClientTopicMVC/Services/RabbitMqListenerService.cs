using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using RabbitMQTopicHWReceiverSection.Models;
using System.Text;
using ClientTopicMVC.Models;
using Microsoft.AspNetCore.Connections;

public class RabbitMqListenerService : BackgroundService
{
    private string _routingKey = "";
    public static List<string> Messages { get; set; } = new List<string>();

    private readonly List<MessageViewModel> _messages = new List<MessageViewModel>();

    public void SetRoutingKey(string routingKey)
    {
        _routingKey = routingKey;
    }

    public List<MessageViewModel> GetMessages()
    {
        return _messages.ToList();
    }

    public async Task StartListening()
    {
        await ExecuteAsync(CancellationToken.None);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri("amqps://usnletdq:FKYHW66StW-A21THXjGj_sIzy7Kn14ue@dog.lmq.cloudamqp.com/usnletdq")
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        string exchangeName = "main_folder";
        await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct);

        string queueName = (await channel.QueueDeclareAsync()).QueueName;
        await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: _routingKey+".#");

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _messages.Add(new MessageViewModel
            {
                Message = message,
                ReceivedAt = DateTime.Now
            });



            Console.WriteLine($"msg->: {message}");

            Messages.Add(message);

            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
