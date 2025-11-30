using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OrdersMicroservice.API.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(IOrdersService ordersService) : ControllerBase
    {
        private readonly IOrdersService _ordersService = ordersService;

        // GET: api/Orders
        [HttpGet]
        public async Task<IEnumerable<OrderResponse?>> GetOrders()
        {
            List<OrderResponse?> orders = await _ordersService.GetOrders();
            return orders;
        }

        // GET: api/Orders/search/orderid/{orderId}
        [HttpGet("search/orderid/{orderId}")]
        public async Task<OrderResponse?> GetOrderByOrderId(Guid orderId)
        {
            FilterDefinition<Order> filter =
                Builders<Order>.Filter.Eq(o => o.OrderID, orderId);

            OrderResponse? order = await _ordersService.GetOrderByCondition(filter);
            return order;
        }

        // GET: api/Orders/search/productid/{productId}
        [HttpGet("search/productid/{productId}")]
        public async Task<IEnumerable<OrderResponse?>> 
            GetOrdersByProductId(Guid productId)
        {
            FilterDefinition<Order> filter =
                Builders<Order>.Filter.ElemMatch(o => o.OrderItems,
                Builders<OrderItem>.Filter.Eq(p => p.ProductID, productId));

            List<OrderResponse?> orders = await 
                _ordersService.GetOrdersByCondition(filter);
            return orders;
        }

        // GET: api/Orders/search/orderdate/{orderDate}
        [HttpGet("search/orderdate/{orderDate}")]
        public async Task<IEnumerable<OrderResponse?>> 
            GetOrdersByOrderDate(DateTime orderDate)
        {
            FilterDefinition<Order> filter =
                Builders<Order>.Filter.Eq(o =>
                o.OrderDate.ToString("yyyy-MM-dd"), orderDate.ToString("yyyy-MM-dd"));

            List<OrderResponse?> orders = await 
                _ordersService.GetOrdersByCondition(filter);
            return orders;
        }

        // GET: api/Orders/search/userid/{userId}
        [HttpGet("search/userid/{userId}")]
        public async Task<IEnumerable<OrderResponse?>> 
            GetOrdersByUserId(Guid userId)
        {
            FilterDefinition<Order> filter =
                Builders<Order>.Filter.Eq(o =>
                o.UserID, userId);

            List<OrderResponse?> orders = await 
                _ordersService.GetOrdersByCondition(filter);
            return orders;
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<IActionResult> Post(OrderAddRequest orderAddRequest)
        {
            if(orderAddRequest == null)
            {
                return BadRequest("Invalid order data.");
            }

           OrderResponse? orderResponse =  await 
                _ordersService.AddOrder(orderAddRequest);

            if (orderResponse == null)
                return Problem("Error in adding order.");

            return Created($"api/Orders/search/orderid/{orderResponse.OrderID}",
                orderResponse.ToString());  
        }

        // PUT: api/Orders/{orderId}
        [HttpPut("{orderId}")]
        public async Task<IActionResult> Put(Guid orderId,
            OrderUpdateRequest orderUpdateRequest)
        {
            if (orderUpdateRequest == null)
            {
                return BadRequest("Invalid order data.");
            }

            if(orderId!= orderUpdateRequest.OrderID)
            {
                return BadRequest("Order ID mismatch with the OrderID from request body.");
            }

            OrderResponse? orderResponse = await
                 _ordersService.UpdateOrder(orderUpdateRequest);

            if (orderResponse == null)
                return Problem("Error in updating order.");

            return Ok(orderResponse);
        }

        // DELETE: api/Orders/{orderId}
        [HttpDelete("{orderId}")]
        public async Task<IActionResult> Delete(Guid orderId)
        {
            if (orderId == Guid.Empty)
            {
                return BadRequest("Invalid Order ID.");
            }

            bool isDeleted = await
                 _ordersService.DeleteOrder(orderId);

            if (!isDeleted)
                return Problem("Error in deleting order.");

            return Ok(isDeleted);
        }
    }
}
