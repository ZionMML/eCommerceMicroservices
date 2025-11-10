using AutoMapper;
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using System.Linq.Expressions;

namespace eCommerce.BusinessLogicLayer.Services;

internal class ProductsService : IProductsService
{
    private readonly IValidator<ProductAddRequest> _productAddRequestValidator;
    private readonly IValidator<ProductUpdateRequest> _productUpdateRequestValidator;
    private readonly IMapper _mapper;
    private readonly IProductsRepository _productsRepository;

    public ProductsService(IValidator<ProductAddRequest> productAddRequestValidator,
        IValidator<ProductUpdateRequest> productUpdateRequestValidator,
        IMapper mapper,
        IProductsRepository productsRepository)
    {
        _productAddRequestValidator = productAddRequestValidator;
        _productUpdateRequestValidator = productUpdateRequestValidator;
        _mapper = mapper;
        _productsRepository = productsRepository;
    }

    public async Task<ProductResponse?> AddProduct(ProductAddRequest
        productAddRequest)
    {
        ArgumentNullException.ThrowIfNull(productAddRequest);

        // Validate the product add request
        FluentValidation.Results.ValidationResult validationResult =
            await _productAddRequestValidator.ValidateAsync(productAddRequest);

        if (!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(temp => temp.ErrorMessage));

            throw new ArgumentException(errors);
        }

        // Attempt to add product
        Product productInput = _mapper.Map<Product>(productAddRequest);
        Product? addedProduct = await _productsRepository.AddProduct(productInput);

        if (addedProduct is null)
        {
            return null;
        }

        ProductResponse addedProductResponse = _mapper.Map<ProductResponse>(addedProduct);

        return addedProductResponse;
    }

    public async Task<bool> DeleteProduct(Guid productId)
    {
        Product? existingProdduct = await
             _productsRepository.GetProductByCondition(temp =>
             temp.ProductID == productId);

        if (existingProdduct is null)
        {
            return false;
        }

        // Attempt to delete product
        bool isDeleted = await _productsRepository.DeleteProduct(productId);
        return isDeleted;
    }

    public async Task<ProductResponse?> GetProductByCondition
        (Expression<Func<Product, bool>> conditionExpression)
    {
        Product? product = await _productsRepository.GetProductByCondition
              (conditionExpression);

        if (product is null)
        {
            return null;
        }

        ProductResponse productResponse = _mapper.Map<ProductResponse>(product);
        // Invokes ProductToProductResponseMappingProfile

        return productResponse;
    }

    public async Task<List<ProductResponse?>> GetProducts()
    {
        IEnumerable<Product>? products = await _productsRepository.GetProducts();

        IEnumerable<ProductResponse> productResponses = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductResponse>>(products);
        // Invokes ProductToProductResponseMappingProfile

        return [.. productResponses];
    }

    public async Task<List<ProductResponse?>> GetProductsByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        IEnumerable<Product>? products = await _productsRepository.GetProductsByCondition(conditionExpression);

        IEnumerable<ProductResponse> productResponses = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductResponse>>(products);
        // Invokes ProductToProductResponseMappingProfile

        return [.. productResponses];
    }

    public async Task<ProductResponse?> UpdateProduct(ProductUpdateRequest productUpdateRequest)
    {
       Product? existingProduct = await _productsRepository.GetProductByCondition(temp =>
            temp.ProductID == productUpdateRequest.ProductID);  

        if(existingProduct is null){
            throw new ArgumentException("Invalid Product ID");
        }

        // Validate the product using Fluent Validation
        FluentValidation.Results.ValidationResult validationResult = await
             _productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

        if (!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(temp => temp.ErrorMessage));

            throw new ArgumentException(errors);
        }

        // Map from ProductUpdateRequest to Product type
        Product product = _mapper.Map<Product>(productUpdateRequest); // Invokes
        //ProductionUpdateRequestToProductMappingProfile

        Product? updatedProduct = await _productsRepository.UpdateProduct(product);

        ProductResponse? updatedProductResponse = 
            _mapper.Map<ProductResponse>(updatedProduct);

        return updatedProductResponse;
    }
}
