using eCommerce.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.BusinessLogicLayer.Validators;

public class ProductAddRequestValidator : AbstractValidator<ProductAddRequest>
{
    public ProductAddRequestValidator()
    {
        // ProductName
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.");

        // Category
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category is incorrect.");

        // UnitPrice
        RuleFor(x => x.UnitPrice)
            .InclusiveBetween(0, double.MaxValue)
            .WithMessage($"Unit price must be between 0 to {double.MaxValue}.");

        // QuantityInStock
        RuleFor(x => x.QuantityInStock)
            .InclusiveBetween(0, int.MaxValue)
            .WithMessage($"Quantity in stock must be between 0 to {int.MaxValue}.");
    }
}

