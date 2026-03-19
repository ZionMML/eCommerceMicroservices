using Microsoft.Extensions.Configuration;

namespace eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly IConfiguration _configuration;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            _configuration = configuration;

            string hostName = _configuration["RabbitMQ_HostName"]!;
            string userName = _configuration["RabbitMQ_UserName"]!; 
            string password = _configuration["RabbitMQ_Password"]!;
            string port = _configuration["RabbitMQ_Port"]!;
        }
        public void Publish<T>(string routingKey, T message)
        {
            throw new NotImplementedException();
        }
    }
}
