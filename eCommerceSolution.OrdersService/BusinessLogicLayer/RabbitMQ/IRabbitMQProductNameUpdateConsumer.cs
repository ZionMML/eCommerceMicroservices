
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQProductNameUpdateConsumer
    {
        Task Consume();
    }
}