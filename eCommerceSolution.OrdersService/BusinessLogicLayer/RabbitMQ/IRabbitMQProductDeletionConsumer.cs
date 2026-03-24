
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQProductDeletionConsumer
    {
        Task Consume();
        void Dispose();
    }
}