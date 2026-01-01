using eCommerce.UsersMicroservice.BusinessLogicLayer.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net.Http.Json;
using System.Text.Json;

namespace eCommerce.UsersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient(HttpClient httpClient,
    ILogger<ProductsMicroserviceClient> logger,
    IDistributedCache cache)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger = logger;
    private readonly IDistributedCache _cache = cache;

    public async Task<ProductDTO?> GetProductByProdcutID(Guid productID)
    {
        try
        {

            string cachekey = $"product:{productID}";
            string? cachedProduct = await _cache.GetStringAsync(cachekey);

            if (cachedProduct != null)
            {
                ProductDTO? cachedProductDTO =
                  JsonSerializer.Deserialize<ProductDTO>(cachedProduct);

                if (cachedProductDTO != null)
                    return cachedProductDTO;
            }

            var response = await _httpClient.
            GetAsync($"/api/products/search/product-id/{productID}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null,
                        System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    throw new HttpRequestException($"Http request failed with" +
                        $"status code {response.StatusCode}");
                }
            }

            var product = await response.Content.ReadFromJsonAsync<ProductDTO>();

            if (product == null)
            {
                throw new ArgumentException("Invalid Product ID");
            }

            string productJson =
                JsonSerializer.Serialize(product);

            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(300))
                .SetSlidingExpiration(TimeSpan.FromSeconds(100));

            string cacheKey = $"product:{product.ProductID}";

            await _cache.SetStringAsync(cacheKey, productJson, cacheOptions);

            return product;
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogError("Bulkhead Isolation limit reached, returning null. " +
                "Exception: {Exception}", ex);

            return new ProductDTO
            (
                ProductID: Guid.Empty,
                ProductName: "Temporarily Unavailable (bulkhead isolation)",
                Category: "Temporarily Unavailable (bulkhead isolation)",
                UnitPrice: 0.0,
                QuantityInStock: 0
            );
        }
    }
}