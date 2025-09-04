using System.Net.Http.Json;
using System.Security.Claims;
using Backend.User.Enums;
using Backend.User.Models;

namespace Backend.User.HttpsClient
{
    public class StoreApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StoreApiService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public StoreApiService(HttpClient httpClient, ILogger<StoreApiService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<StoreResponseModel> RegisterStoreAsync(RegisterStoreModel model)
        {
            try
            {
                // 
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("No JWT token found in ClaimsPrincipal");
                    return new StoreResponseModel
                    {
                        Success = false,
                        Message = OperationResult.Error,
                        ErrorMessage = "Missing JWT token"
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("api/store", model);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Store API returned error {StatusCode}", response.StatusCode);

                    return new StoreResponseModel
                    {
                        Success = false,
                        Message = OperationResult.Failed,
                        ErrorMessage = $"Store API returned {response.StatusCode}"
                    };
                }

                var storeResponse = await response.Content.ReadFromJsonAsync<StoreResponseModel>();

                return storeResponse ?? new StoreResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = "Failed to parse store API response"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Store API");

                return new StoreResponseModel
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = "Exception while calling Store API"
                };
            }
        }
    }
}
