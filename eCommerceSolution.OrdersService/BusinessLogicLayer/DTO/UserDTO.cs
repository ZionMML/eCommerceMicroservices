namespace eCommerce.UsersMicroservice.BusinessLogicLayer.DTOs;

public record UserDTO(Guid UserID, string? Email,
    string? PersonName, string Gender);

