using System.Net.Http.Json;
using System.Security.Claims;
using Backend.User.Enums;
using Backend.User.Models;

namespace Backend.User.HttpsClients
{
    public class StoreApiClient : IStoreApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StoreApiClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public StoreApiClient(HttpClient httpClient, ILogger<StoreApiClient> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserApiResponse<object>> RegisterStoreAsync(RegisterStoreModel model)
        {
            try
            {
                // 
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("No JWT token found in ClaimsPrincipal");
                    return new UserApiResponse<object>
                    {
                        StatusCode = 401,
                        Message = "Missing JWT token",
                        Data = null
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("api/store", model);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Store API returned error {StatusCode}", response.StatusCode);

                    return new UserApiResponse<object>
                    {
                        StatusCode = (int)response.StatusCode,
                        Message = $"Store API returned {response.StatusCode}",
                        Data = null
                    };
                }

                var storeResponse = await response.Content.ReadFromJsonAsync<UserApiResponse<object>>();

                return storeResponse ?? new UserApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "Failed to parse store API response",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Store API");

                return new UserApiResponse<object>
                {
                    StatusCode = 500,
                    Message = "Exception while calling Store API",
                    Data = null
                };
            }
        }
    }
}
