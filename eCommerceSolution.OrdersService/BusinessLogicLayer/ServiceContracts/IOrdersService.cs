using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;

public interface IOrdersService
{
    /// <summary>
    /// Retrieves the list of orders from the orders repository
    /// </summary>
    /// <returns>Returns the list of OrderResponse objects</returns>
    Task<List<OrderResponse?>>GetOrders();

    /// <summary>
    /// Retrieves the list of orders matching with given condition
    /// </summary>
    /// <param name="filter">Expression that represents condition to check</param>
    /// <returns>Returns matching orders as OrderResponse objects</returns>
    Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter);

    /// <summary>
    /// Retrieves the order matching with given condition
    /// </summary>
    /// <param name="filter">Expression that represents condition to check</param>
    /// <returns>Returns matching order as OrderResponse objects</returns>
    Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter);

    /// <summary>
    /// Add order into the collection using orders repository
    /// </summary>
    /// <param name="orderAddRequest">Order to insert</param>
    /// <returns>Returns OrderResponse object that contains order details
    /// after inserting or returns null if insertion is uncessful.</returns>
    Task<OrderResponse?>AddOrder(OrderAddRequest orderAddRequest);

    /// <summary>
    /// update order into the collection using orders repository
    /// </summary>
    /// <param name="orderUpdateRequest">Order to update/param>
    /// <returns>Returns OrderResponse object that contains order details
    /// after updating or returns null if insertion is uncessful.</returns>
    Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest);

    /// <summary>
    /// Deletes an existing order based on given order id
    /// </summary>
    /// <param name="orderID">OrderID to search and delete</param>
    /// <returns>Returns true if the deletion is successful; otherwise false</returns>
    Task<bool> DeleteOrder(Guid orderID);

}
