using eCommerce.UsersMicroservice.BusinessLogicLayer.DTOs;
using System.Net.Http.Json;

namespace eCommerce.UsersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<ProductDTO?> GetProductByProdcutID(Guid productID)
    {
        var response = await _httpClient.
            GetAsync($"/api/products/search/product-id/{productID}");

        if (!response.IsSuccessStatusCode)
        {
           if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
           {
                return null;
            }
           else if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
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

        if(product == null)
        {
            throw new ArgumentException("Invalid Product ID");
        }

        return product;
    }
}