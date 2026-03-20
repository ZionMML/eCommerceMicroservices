
namespace eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQPublisher
    {
        Task Publish<T>(string routingKey, T message);
    }
}
