using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

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
        public void Publish<T>(string routingKey, T message)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }   
    }
}
