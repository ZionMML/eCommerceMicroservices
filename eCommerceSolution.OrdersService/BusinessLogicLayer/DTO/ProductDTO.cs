namespace eCommerce.UsersMicroservice.BusinessLogicLayer.DTOs;

public record ProductDTO(Guid ProductID, string? ProductName,
    string? Category, double UnitPrice, int QuantityInStock);