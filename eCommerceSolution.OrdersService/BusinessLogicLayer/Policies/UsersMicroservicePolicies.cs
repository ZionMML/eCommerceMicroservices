using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
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
             TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
             onRetry: (outcome, timespan, retryAttempt, context) =>
             {
                 _logger.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, " +
                     $"then making retry {retryAttempt}.");
             });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy =
       Polly.Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 3,
            durationOfBreak: TimeSpan.FromMinutes(2),
            onBreak: (outcome, timespan) =>
            {
                _logger.LogWarning($"Circuit breaker opened for " +
                    $"{timespan.TotalMinutes} minutes due to consecutive failures." +
                    "The subsequent requests will be blocked.");
            }, onReset: () =>
            {
                _logger.LogInformation("Circuit breaker closed. Requests are " +
                    "allowed again.");
            });

        return policy;
    }
}