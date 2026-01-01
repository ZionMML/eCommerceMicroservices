using eCommerce.UsersMicroservice.BusinessLogicLayer.DTOs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;

namespace eCommerce.UsersMicroservice.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient(HttpClient httpClient,
    ILogger<UsersMicroserviceClient> logger,
    IDistributedCache cache)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger = logger;
    private readonly IDistributedCache _cache = cache;

    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
        try
        {
            string cachekey = $"user:{userID}";
            string? cachedUser = await _cache.GetStringAsync(cachekey);

            if (cachedUser != null)
            {
                UserDTO? cachedUserDTO =
                  System.Text.Json.JsonSerializer
                  .Deserialize<UserDTO>(cachedUser);
                if (cachedUserDTO != null)
                    return cachedUserDTO;
            }

            var response = await _httpClient.GetAsync($"/api/users/{userID}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    UserDTO? fallbackUser = await
                        response.Content.ReadFromJsonAsync<UserDTO>();

                    if (fallbackUser == null)
                    {
                        throw new NotImplementedException
                           ("Fallback policy was not implemented");
                    }

                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
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
                    //throw new HttpRequestException($"Http request failed with" +
                    //    $"status code {response.StatusCode}");

                    return new UserDTO(PersonName: "Temporarily Unavailable",
                        Email: "Temporarily Unavailable",
                        Gender: "Temporarily Unavailable",
                        UserID: Guid.Empty);
                }
            }

            var user = await response.Content.ReadFromJsonAsync<UserDTO>();

            if (user == null)
            {
                throw new ArgumentException("Invalid User ID");
            }

            var cacheKey = $"user:{userID}";
            string userJson = System.Text.Json.JsonSerializer
                .Serialize(user);
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            await _cache.SetStringAsync(cacheKey, userJson, cacheOptions);

            return user;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError("Circuit breaker is open, returning dummy user data. Exception: {Exception}", ex);

            return new UserDTO(PersonName: "Temporarily Unavailable (circuit breaker)",
                        Email: "Temporarily Unavailable (circuit breaker)",
                        Gender: "Temporarily Unavailable (circuit breaker)",
                        UserID: Guid.Empty);
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError("Timeout occured while fetching user data, " +
                "returning dummy user data.");

            return new UserDTO(PersonName: "Temporarily Unavailable (timeout)",
                        Email: "Temporarily Unavailable (timeout)",
                        Gender: "Temporarily Unavailable (timeout)",
                        UserID: Guid.Empty);
        }
    }
}