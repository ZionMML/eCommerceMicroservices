using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;

public class OrderUpdateRequestValidator : AbstractValidator<OrderUpdateRequest>
{
    public OrderUpdateRequestValidator()
    {
        RuleFor(temp => temp.OrderID)
            .NotEmpty().WithErrorCode("Order ID cannot be blank.");

        RuleFor(temp => temp.UserID)
            .NotEmpty().WithErrorCode("User ID cannot be blank.");

        RuleFor(temp => temp.OrderDate)
            .NotEmpty().WithErrorCode("Ordder Date cannot be blank.");

        RuleFor(temp => temp.OrderItems)
            .NotEmpty().WithErrorCode("Order items cannot be blank.");
    }
}