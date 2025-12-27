using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using eCommerce.UsersMicroservice.BusinessLogicLayer.DTOs;
using eCommerce.UsersMicroservice.BusinessLogicLayer.HttpClients;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;

public class OrdersService(IOrdersRepository ordersRepository, IMapper mapper,
    IValidator<OrderAddRequest> orderAddRequestValidator,
    IValidator<OrderItemAddRequest> orderItemAddRequestValidator,
    IValidator<OrderUpdateRequest> orderUpdateRequestValidator,
    IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator,
    UsersMicroserviceClient usersMicroserviceClient,
    ProductsMicroserviceClient productsMicroserviceClient
        ) : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository = ordersRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator = orderAddRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator = orderItemAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator = orderUpdateRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
    private readonly UsersMicroserviceClient _usersMicroserviceClient = usersMicroserviceClient;
    private readonly ProductsMicroserviceClient _productsMicroserviceClient = productsMicroserviceClient;
    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        ArgumentNullException.ThrowIfNull(orderAddRequest);

        ValidationResult orderAddRequestValidationResult =
             await _orderAddRequestValidator.ValidateAsync(orderAddRequest);

        if (!orderAddRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderAddRequestValidationResult.Errors
                .Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }
      
        List<ProductDTO> products = new List<ProductDTO>();

        foreach (OrderItemAddRequest item in orderAddRequest.OrderItems)
        {
            ValidationResult orderItemAddRequestValidationResult =
             await _orderItemAddRequestValidator.ValidateAsync(item);

            if (!orderItemAddRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemAddRequestValidationResult.Errors
                    .Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }

            //TO DO: Add logic for checking if ProductID exists in Products microservice
            ProductDTO? product = await _productsMicroserviceClient.GetProductByProdcutID(item.ProductID);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {item.ProductID} does not exist.");
            }

            products.Add(product);
        }

        // TO DO: Add logic for checking if UserID exists in Users microservice
        UserDTO? user = await
            _usersMicroserviceClient.GetUserByUserID(orderAddRequest.UserID);

        if (user == null)
        {
            throw new ArgumentException($"User with ID {orderAddRequest.UserID} does not exist.");
        }

        Order order = _mapper.Map<Order>(orderAddRequest);

        foreach (var item in order.OrderItems)
        {
            item.TotalPrice = item.Quantity * item.UnitPrice;
        }

        order.TotalBill = order.OrderItems.Sum(item => item.TotalPrice);

        Order? addedOrder = await _ordersRepository.AddOrder(order);

        if (addedOrder == null)
        {
            return null;
        }

        OrderResponse orderResponse = _mapper.Map<OrderResponse>(addedOrder);

        //TO DO: Load ProductName and Category in OrderItemResponse
        if (orderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = products.Where(temp=>
                temp.ProductID == orderItemResponse.ProductID).FirstOrDefault();

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO,
                    orderItemResponse);
            }
        }

        return orderResponse;
    }



    public async Task<bool> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Order> filter =
             Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);

        Order? existingOrder = await _ordersRepository.GetOrderByCondition(filter);

        if (existingOrder == null)
        {
            throw new ArgumentException($"Order with ID {orderID} does not exist.");
        }

        bool isDeleted = await _ordersRepository.DeleteOrder(orderID);
        return isDeleted;

    }

    public async Task<OrderResponse?> GetOrderByCondition
        (FilterDefinition<Order> filter)
    {
        Order? order = await _ordersRepository.GetOrderByCondition(filter);

        if (order == null)
        {
            return null;
        }

        OrderResponse orderResponse = _mapper.Map<OrderResponse>(order);

        //TO DO: Load ProductName and Category from Products microservice
        if (orderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await
                     _productsMicroserviceClient.GetProductByProdcutID
                     (orderItemResponse.ProductID);

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO,
                    orderItemResponse);
            }
        }

        return orderResponse;
    }

    public async Task<List<OrderResponse?>> GetOrders()
    {
        IEnumerable<Order?> orders = await _ordersRepository.GetOrders();
        if (orders == null)
        {
            return null;
        }
        IEnumerable<OrderResponse?> orderResponses =
            _mapper.Map<IEnumerable<OrderResponse>>(orders);

        //TO DO: Load ProductName and Category from Products microservice
        //for each OrderItemResponse in each OrderResponse
        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse == null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await
                     _productsMicroserviceClient.GetProductByProdcutID
                     (orderItemResponse.ProductID);

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO,
                    orderItemResponse);
            }


        }
        return (List<OrderResponse?>)orderResponses;
    }

    public async Task<List<OrderResponse?>> GetOrdersByCondition
        (FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await _ordersRepository
            .GetOrdersByCondition(filter);

        if (orders == null)
        {
            return null;
        }

        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);

        //TO DO: Load ProductName and Category from Products microservice
        //for each OrderItemResponse in each OrderResponse
        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse == null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = await
                     _productsMicroserviceClient.GetProductByProdcutID
                     (orderItemResponse.ProductID);

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO,
                    orderItemResponse);
            }


        }

        return (List<OrderResponse?>)orderResponses;
    }

    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        ArgumentNullException.ThrowIfNull(orderUpdateRequest);

        ValidationResult orderUpdateRequestValidationResult =
             await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);

        if (!orderUpdateRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors
                .Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        List<ProductDTO> products = new List<ProductDTO>();

        foreach (OrderItemUpdateRequest item in orderUpdateRequest.OrderItems)
        {
            ValidationResult orderItemUpdateRequestValidationResult =
             await _orderItemUpdateRequestValidator.ValidateAsync(item);

            if (!orderItemUpdateRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemUpdateRequestValidationResult.Errors
                    .Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }

            //TO DO: Add logic for checking if ProductID exists in Products microservice
            ProductDTO? product = await _productsMicroserviceClient.GetProductByProdcutID(item.ProductID);
            if (product == null)
            {
                throw new ArgumentException($"Product with ID {item.ProductID} does not exist.");
            }

            products.Add(product);
        }

        // TO DO: Add logic for checking if UserID exists in Users microservice
        UserDTO? user = await
            _usersMicroserviceClient.GetUserByUserID(orderUpdateRequest.UserID);

        if (user == null)
        {
            throw new ArgumentException($"User with ID {orderUpdateRequest.UserID} does not exist.");
        }

        Order order = _mapper.Map<Order>(orderUpdateRequest);

        foreach (var item in order.OrderItems)
        {
            item.TotalPrice = item.Quantity * item.UnitPrice;
        }

        order.TotalBill = order.OrderItems.Sum(item => item.TotalPrice);

        Order? updatedOrder = await _ordersRepository.UpdateOrder(order);

        if (updatedOrder == null)
        {
            return null;
        }

        OrderResponse orderResponse = _mapper.Map<OrderResponse>(updatedOrder);

        //TO DO: Load ProductName and Category in OrderItemResponse
        if (orderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? productDTO = products.Where(temp =>
                temp.ProductID == orderItemResponse.ProductID).FirstOrDefault();

                if (productDTO == null)
                    continue;

                _mapper.Map<ProductDTO, OrderItemResponse>(productDTO,
                    orderItemResponse);
            }
        }

        return orderResponse;
    }
}