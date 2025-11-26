using eCommerce.OrdersService.DataAccessLayer.Entities;
using eCommerce.OrdersService.DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;

namespace eCommerce.OrdersService.DataAccessLayer.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly IMongoCollection<Order> _ordersCollection;

    private readonly string _collectionName = "orders";
    public OrdersRepository(IMongoDatabase database)
    {
        _ordersCollection = database.GetCollection<Order>(_collectionName);
    }
    public async Task<IEnumerable<Order>> GetOrders()
    {
        return await _ordersCollection.Find(_ => true).ToListAsync();
    }
    public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        return await _ordersCollection.Find(filter).ToListAsync();
    }
    public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        return await _ordersCollection.Find(filter).FirstOrDefaultAsync();
    }
    public async Task<Order?> AddOrder(Order order)
    {
        order.OrderID = Guid.NewGuid();

        await _ordersCollection.InsertOneAsync(order);
        return order;
    }
    public async Task<Order?> UpdateOrder(Order order)
    {
        var filter = Builders<Order>.Filter.Eq(o => o.OrderID, order.OrderID);
        var result = await _ordersCollection.ReplaceOneAsync(filter, order);
        return result.ModifiedCount > 0 ? order : null;
    }
    public async Task<bool> DeleteOrder(Guid orderID)
    {
        // Filter definition to find the order by OrderID
        var filter = Builders<Order>.Filter.Eq(o => o.OrderID, orderID);
        var result = await _ordersCollection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }
}