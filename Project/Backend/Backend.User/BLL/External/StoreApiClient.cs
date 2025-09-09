using Commons.Enums;
using Commons.Models.Requests;
using Commons.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BLL.External
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
                // Lấy JWT từ request gốc
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("❌ No JWT token found in request headers");
                    return new UserApiResponse<object>
                    {
                        Message = OperationResult.Error,
                        ErrorMessage = "Missing JWT token",
                        Data = null
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("api/store", model);

                // Đọc response body
                var raw = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("👉 Store API raw response ({StatusCode}): {Raw}",
                    (int)response.StatusCode, raw);

                if (!response.IsSuccessStatusCode)
                {
                    return new UserApiResponse<object>
                    {
                        Message = MapStatusCodeToResult((int)response.StatusCode),
                        ErrorMessage = $"Store API returned {(int)response.StatusCode} - {response.ReasonPhrase}",
                        Data = null
                    };
                }

                var storeResponse = await response.Content.ReadFromJsonAsync<UserApiResponse<object>>();

                return storeResponse ?? new UserApiResponse<object>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Failed to parse store API response",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while calling Store API");

                return new UserApiResponse<object>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Exception while calling Store API",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Map HTTP status code về OperationResult
        /// </summary>
        private OperationResult MapStatusCodeToResult(int statusCode) => statusCode switch
        {
            200 => OperationResult.Success,
            400 => OperationResult.Failed,
            401 => OperationResult.Error,
            404 => OperationResult.NotFound,
            409 => OperationResult.Conflict,
            _ => OperationResult.Error
        };
    }
}
