using eCommerce.DataAccessLayer.Entities;
using eCommerce.BusinessLogicLayer.DTO;
using System.Linq.Expressions;

namespace eCommerce.BusinessLogicLayer.ServiceContracts;

public interface IProductsService
{
    /// <summary>
    /// Retrieves the list of products from the products repository
    /// </summary>
    /// <returns>Returns list of ProductResponse objects</returns>
    Task<List<ProductResponse?>> GetProducts();

    /// <summary>
    /// Retrievves list of products matching with given condition
    /// </summary>
    /// <param name="conditionExpression">Expression that represents
    /// condition to check
    /// </param>
    /// <returns>Returns matching products</returns>
    Task<List<ProductResponse?>> GetProductsByCondition(
        Expression<Func<Product, bool>> conditionExpression);

    /// <summary>
    /// Returns a single product matching with given condition
    /// </summary>
    /// <param name="conditionExpression"></param>Expression that represents
    /// condition to check
    /// <returns>Returns matching product or null</returns>
    Task<ProductResponse?> GetProductByCondition(
        Expression<Func<Product, bool>> conditionExpression);

    /// <summary>
    /// Adds (inserts) product into the table using 
    /// </summary>
    /// <param name="productAddRequest"></param>
    /// <returns></returns>i
    Task<ProductResponse?> AddProduct(ProductAddRequest productAddRequest);

    /// <summary>
    /// Updates the existing product based on the Produc
    /// </summary>
    /// <param name="productUpdateRequest"></param>
    /// <returns>Returns product object after suuessful update; otherwise null</returns>
    Task<ProductResponse?> UpdateProduct(ProductUpdateRequest productUpdateRequest);


    /// <summary>
    /// Deletes an existing product based on given product id
    /// </summary>
    /// <param name="productId">ProductID to search and delete</param>
    /// <returns>Returns true if the deletion is successful; </returns>
    Task<bool> DeleteProduct(Guid productId);

}

