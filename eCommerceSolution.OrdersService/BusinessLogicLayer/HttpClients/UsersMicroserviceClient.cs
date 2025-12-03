using eCommerce.UsersMicroservice.BusinessLogicLayer.DTOs;
using System.Net.Http.Json;

namespace eCommerce.UsersMicroservice.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<UserDTO?> GetUserByIDAsync(Guid userID)
    {
        var response = await _httpClient.GetAsync($"/api/users/{userID}");

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

        var user = await response.Content.ReadFromJsonAsync<UserDTO>();

        if(user == null)
        {
            throw new ArgumentException("Invalid User ID");
        }

        return user;
    }
}