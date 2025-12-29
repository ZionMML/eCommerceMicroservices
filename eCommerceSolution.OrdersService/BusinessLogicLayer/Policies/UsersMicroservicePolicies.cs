using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger) : IUsersMicroservicePolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger = logger;

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        AsyncRetryPolicy<HttpResponseMessage> policy =
        Polly.Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
         .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: retryAttempt =>
             TimeSpan.FromSeconds(2),
             onRetry: (outcome, timespan, retryAttempt, context) =>
             {
                 _logger.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, " +
                     $"then making retry {retryAttempt}.");
             });

        return policy;
    }
}