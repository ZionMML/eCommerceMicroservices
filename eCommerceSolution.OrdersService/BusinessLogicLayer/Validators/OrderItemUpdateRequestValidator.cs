using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;

public class OrderItemUpdateRequestValidator : AbstractValidator<OrderItemUpdateRequest>
{
    public OrderItemUpdateRequestValidator()
    {
        RuleFor(temp => temp.ProductID)
            .NotEmpty().WithErrorCode("Product ID cannot be blank.");

        RuleFor(temp => temp.UnitPrice)
            .NotEmpty().WithErrorCode("Unit Price cannot be blank.")
            .GreaterThan(0).WithErrorCode("Unit Price cannot be less than or equal to zero.");

        RuleFor(temp => temp.Quantity)
              .NotEmpty().WithErrorCode("Quantity cannot be blank.")
              .GreaterThan(0).WithErrorCode("Quantity cannot be less than or equal to zero.");

    }
}