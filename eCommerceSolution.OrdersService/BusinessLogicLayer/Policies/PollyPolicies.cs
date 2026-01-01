using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class PollyPolicies(ILogger<PollyPolicies> logger) : IPollyPolicies
{
    private readonly ILogger<PollyPolicies> _logger = logger;

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        AsyncRetryPolicy<HttpResponseMessage> policy =
        Polly.Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
         .WaitAndRetryAsync(retryCount: retryCount, sleepDurationProvider: retryAttempt =>
             TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
             onRetry: (outcome, timespan, retryAttempt, context) =>
             {
                 _logger.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, " +
                     $"then making retry {retryAttempt}.");
             });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
        int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy =
       Polly.Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking:
            handledEventsAllowedBeforeBreaking,
            durationOfBreak: durationOfBreak,
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

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeoutDuration)
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy =
              Policy.TimeoutAsync<HttpResponseMessage>(timeoutDuration);

        return policy;
    }
}
