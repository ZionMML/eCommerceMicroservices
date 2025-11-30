using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;

public interface IOrdersRepository
{
    /// <summary>
    /// Retrieves all Orders asynchronously.
    /// </summary>
    /// <returns>Returns all orders from the orders collection</returns>
    Task<IEnumerable<Order>> GetOrders();

    /// <summary>
    /// Retrieves Orders based on a specified filter condition asynchronously.
    /// </summary>
    /// <param name="filter">The condition to filter orders</param>
    /// <returns>Returns a collection of matching orders</returns>
    Task<IEnumerable<Order?>> GetOrdersByCondition
        (FilterDefinition<Order> filter);

    /// <summary>
    /// Retrieves a single Order based on a specified filter condition asynchronously.
    /// </summary>
    /// <param name="filter">The condition to filter order</param>
    /// <returns>Returns a matching order</returns>
    Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter);

    /// <summary>
    /// Adds a new Order asynchronously.
    /// </summary>
    /// <param name="order">The order to be added</param>
    /// <returns>Returns the added Order object or null if unsuccessful</returns>
    Task<Order?> AddOrder(Order order);

    /// <summary>
    /// Updates the existing order asynchronously.
    /// </summary>
    /// <param name="order">The order to be updated.</param>
    /// <returns>Returns the updated order object or null if not found.</returns>
    Task<Order?> UpdateOrder(Order order);

    /// <summary>
    /// Deletes the order asynchronously.
    /// </summary>
   /// <param name="orderID">The Order ID based on which we need to delete the order.</param>
    /// <returns>Returns true if the deletion is succcessful, false otherwise</returns>
    Task<bool> DeleteOrder(Guid orderID);


}