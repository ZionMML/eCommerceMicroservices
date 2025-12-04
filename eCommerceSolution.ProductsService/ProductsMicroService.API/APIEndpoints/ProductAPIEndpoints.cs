using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using FluentValidation;
using FluentValidation.Results;

namespace eCommerce.ProductsMicroService.API.APIEndpoints;

public static class ProductAPIEndpoints
{
    public static IEndpointRouteBuilder MapProductAPIEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /api/products
        app.MapGet("/api/products", async (IProductsService productService) =>
        {
            var products = await productService.GetProducts();
            return Results.Ok(products);
        });

        // GET /api/products/search/product-id/{ProductId}
        app.MapGet("/api/products/search/product-id/{ProductId:guid}",
            async (IProductsService productService, Guid ProductId) =>
        {
            ProductResponse? product = await 
            productService.GetProductByCondition(temp
                => temp.ProductID == ProductId);

            if(product == null)
                return Results.NotFound();

            return Results.Ok(product);
        });

        // GET /api/products/search/{SearchString}
        app.MapGet("/api/products/search/{SearchString}",
            async (IProductsService productService, string SearchString) =>
            {
                List<ProductResponse?> productsByProductName = await
                    productService.GetProductsByCondition(temp
                    => temp.ProductName != null && temp.ProductName.Contains
                    (SearchString, StringComparison.OrdinalIgnoreCase));

                List<ProductResponse?> productsByCategory = await
                    productService.GetProductsByCondition(temp
                    => temp.Category != null && temp.Category.Contains
                    (SearchString, StringComparison.OrdinalIgnoreCase));

                var products = productsByProductName.Union(productsByCategory);

                return Results.Ok(products);
            });

        // POST /api/products
        app.MapPost("/api/products",
            async (IProductsService productService,
            IValidator<ProductAddRequest> productAddRequestValidator,
            ProductAddRequest productAddRequest) =>
            {
                // Validate the ProductAddRequest
                ValidationResult validationResult = await
                productAddRequestValidator.ValidateAsync(productAddRequest);

                // Check the validation result
                if (!validationResult.IsValid)
                {
                    Dictionary<string, string[]> errors =
                    validationResult.Errors.GroupBy(err => err.PropertyName)
                       .ToDictionary(
                           grp => grp.Key,
                           grp => grp.Select(err => err.ErrorMessage).ToArray()
                       );

                    return Results.ValidationProblem(errors);
                }

                var addedProductResponse = await 
                productService.AddProduct(productAddRequest);

                if (addedProductResponse != null)
                    return Results.Created($"/api/products/search/produc-id/" +
                        $"{addedProductResponse.ProductID}", addedProductResponse);
              
                return Results.Problem("Failed to add the product.");
            });

        // PUT /api/products
        app.MapPut("/api/products",
            async (IProductsService productService,
            IValidator<ProductUpdateRequest> productUpdateRequestValidator,
            ProductUpdateRequest productUpdateRequest) =>
            {
                // Validate the ProductUpdateRequest
                ValidationResult validationResult = await
                productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

                // Check the validation result
                if (!validationResult.IsValid)
                {
                    Dictionary<string, string[]> errors =
                    validationResult.Errors.GroupBy(err => err.PropertyName)
                       .ToDictionary(
                           grp => grp.Key,
                           grp => grp.Select(err => err.ErrorMessage).ToArray()
                       );

                    return Results.ValidationProblem(errors);
                }

                var updatedProductResponse = await 
                productService.UpdateProduct(productUpdateRequest);

                if (updatedProductResponse != null)
                    return Results.Ok(updatedProductResponse);

                return Results.Problem("Failed to update the product.");
            });

        // DELETE /api/products/{ProductId}
        app.MapDelete("/api/products/{ProductId}",
            async (IProductsService productService,Guid ProductId) =>
            {
                bool isDeleted = await 
                productService.DeleteProduct(ProductId);

                if (isDeleted)
                    return Results.Ok(true);

                return Results.Problem("Failed to delete the product.");
            });

        return app;
    }
}

