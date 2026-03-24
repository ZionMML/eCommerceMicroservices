using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeletionConsumer : IDisposable, 
    IRabbitMQProductDeletionConsumer
{
    private readonly IConfiguration _configuration;
    private IConnection _connection;
    private IChannel _channel;
    private readonly ILogger<RabbitMQProductDeletionConsumer> _logger;

    public RabbitMQProductDeletionConsumer(IConfiguration configuration,
        ILogger<RabbitMQProductDeletionConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;

        InitializeRabbitMQAsync().GetAwaiter().GetResult();
    }

    public async Task InitializeRabbitMQAsync()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ_HostName"]!,
            UserName = _configuration["RabbitMQ_UserName"]!,
            Password = _configuration["RabbitMQ_Password"]!,
            Port = Convert.ToInt32(_configuration["RabbitMQ_Port"]!)
        };

        _connection = await factory.CreateConnectionAsync();

        _channel = await _connection.CreateChannelAsync();
    }
    public async Task Consume()
    {
        string routingKey = "product.delete";

        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
        string queueName = _configuration["RabbitMQ_Products_Delete_Queue"]!;

        //Create exchange
        await _channel.ExchangeDeclareAsync(exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true);

        //Create queue
        await _channel.QueueDeclareAsync(queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        //Bind queue to exchange
        await _channel.QueueBindAsync(queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey);

        AsyncEventingBasicConsumer consumer = new(_channel);

        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            ProductDeletionMessage? productDeletionMessage =
              JsonSerializer.Deserialize<ProductDeletionMessage>(message);

            if (productDeletionMessage != null)
            {
                _logger.LogInformation($"Product deleted: {productDeletionMessage.ProductID}," +
                $"Deleted product name:{productDeletionMessage.ProductName}");
            }


        };

        await _channel.BasicConsumeAsync(queue: queueName,
            autoAck: true,
            consumer: consumer);

    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
