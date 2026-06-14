using Amazon.Runtime.Internal.Util;
using eCommerce.UsersMicroservice.BusinessLogicLayer.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQProductNameUpdateConsumer : IDisposable, IRabbitMQProductNameUpdateConsumer
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IChannel _channel;
        private readonly ILogger<RabbitMQProductNameUpdateConsumer> _logger;
        private readonly IDistributedCache _cache;

        public RabbitMQProductNameUpdateConsumer(IConfiguration configuration,
            ILogger<RabbitMQProductNameUpdateConsumer> logger,
            IDistributedCache cache)
        {
            _configuration = configuration;
            _logger = logger;

            InitializeRabbitMQAsync().GetAwaiter().GetResult();
            _cache = cache;
        }

        public async Task InitializeRabbitMQAsync()
        {
            Console.WriteLine("Initializing RabbitMQ Publisher...");
            Console.WriteLine($"RabbitMQ HostName: {_configuration["RabbitMQ_HostName"]}");
            Console.WriteLine($"RabbitMQ UserName: {_configuration["RabbitMQ_UserName"]}");
            Console.WriteLine($"RabbitMQ Port: {_configuration["RabbitMQ_Port"]}");

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
            string routingKey = "product.update.name";

            string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
            string queueName = _configuration["RabbitMQ_Products_Queue"]!;

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

                ProductDTO? productDTO =
                  JsonSerializer.Deserialize<ProductDTO>(message);

                if (productDTO != null)
                {
                    // TO DO: Update product cache
                   await HandleProductUpdate(productDTO);
                }

            };

            await _channel.BasicConsumeAsync(queue: queueName,
                autoAck: true,
                consumer: consumer);

        }

        private async Task HandleProductUpdate(ProductDTO productDTO)
        {
            _logger.LogInformation($"Product name updated: {productDTO.ProductID}," +
                    $"New product name:{productDTO.ProductName}");

            string productJson =
               JsonSerializer.Serialize(productDTO);

            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(300));

            string cacheKey = $"product:{productDTO.ProductID}";

            await _cache.SetStringAsync(cacheKey, productJson, cacheOptions);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
