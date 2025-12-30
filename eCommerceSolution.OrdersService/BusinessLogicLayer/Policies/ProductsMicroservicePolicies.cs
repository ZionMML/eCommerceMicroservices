using eCommerce.UsersMicroservice.BusinessLogicLayer.DTOs;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Fallback;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger) : IProductsMicroservicePolicies
{
    private readonly ILogger<ProductsMicroservicePolicies> _logger = logger;

    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
    {
        AsyncFallbackPolicy<HttpResponseMessage> policy =
        Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
             .FallbackAsync(async (context) =>
             {
                 _logger.LogWarning("Fallback triggered: The request failed, returning" +
                     " dummy data.");

                 ProductDTO product = new(
                        Guid.Empty,
                        "Temporarily Unavailable (fallback)",
                        "Temporarily Unavailable (fallback)",
                        0.0,
                        0
                 );

                 var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                 {
                     Content = new StringContent(
                         System.Text.Json.JsonSerializer.Serialize(product),
                         System.Text.Encoding.UTF8,
                         "application/json")
                 };

                 return response;
             });

        return policy;
    }
}
