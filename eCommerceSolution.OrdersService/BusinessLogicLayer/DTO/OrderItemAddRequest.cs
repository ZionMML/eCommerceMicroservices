namespace eCommerce.OrdersService.BusinessLogicLayer.DTO;

public record OrderItemAddRequest(Guid ProductID, decimal UnitPrice, int Quantity)  
{
    // Parameterless constructor for serialization/deserialization purposes
    public OrderItemAddRequest():this(default, default, default)
    {
        
    }
}