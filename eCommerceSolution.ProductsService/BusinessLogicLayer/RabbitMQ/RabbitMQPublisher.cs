using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            _configuration = configuration;

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
        public async Task Publish<T>(string routingKey, T message)
        {
            string messageJson = JsonSerializer.Serialize(message);
            byte[] messageBodyInBytes = Encoding.UTF8.GetBytes(messageJson);

            string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
            string queueName = _configuration["RabbitMQ_Products_Queue"]!;

            //Create exchange
            await _channel.ExchangeDeclareAsync(exchange: exchangeName,
                type: ExchangeType.Direct,
                durable: true);

            //Create queue
            //await _channel.QueueDeclareAsync(queue: queueName,
            //    durable: true,
            //    exclusive: false,
            //    autoDelete: false,
            //    arguments: null);

            //Bind queue to exchange
            //await _channel.QueueBindAsync(queue: queueName,
            //    exchange: exchangeName,
            //    routingKey: routingKey);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = (DeliveryModes)2 // Persistent
            };

            //Publish message
            await _channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties, 
                body: messageBodyInBytes);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }   
    }
}
