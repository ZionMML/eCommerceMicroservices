using eCommerce.UsersMicroservice.BusinessLogicLayer.DTOs;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Bulkhead;
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
    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy()
    {
        AsyncBulkheadPolicy<HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
            maxParallelization: 2, // number of concurrent executions
            maxQueuingActions: 20, // number of actions that can be queued
            onBulkheadRejectedAsync: context =>
            {
                _logger.LogWarning("Bulkhead Isolation limit reached. " +
                    "The request has been rejected.");
                throw new BulkheadRejectedException("Bulkhead Isolation limit reached.");
            });

        return policy;
    }
}
