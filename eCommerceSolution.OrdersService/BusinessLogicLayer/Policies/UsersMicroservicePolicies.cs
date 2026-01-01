using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger,
    IPollyPolicies pollyPolicies) : IUsersMicroservicePolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger = logger;
    private readonly IPollyPolicies _pollyPolicies = pollyPolicies;

    public IAsyncPolicy<HttpResponseMessage> GetCombiedPolicy()
    {
        var retryPolicy = _pollyPolicies.GetRetryPolicy(5);
        var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(3,TimeSpan.FromMinutes(2));
        var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromSeconds(5));
        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
    }
}