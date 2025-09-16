using User.Common.Enums;
using User.Common.Models.Requests;
using User.Common.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace User.BLL.External
{
    public class StoreApiClient : IStoreApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StoreApiClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StoreApiClient(
            HttpClient httpClient,
            ILogger<StoreApiClient> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserApiResponse<Guid?>> RegisterStoreAsync(RegisterStoreModel model)
        {
            _logger.LogInformation("Calling Store API to register store for UserId: {UserId}", model.UserId);
            try
            {
                // Lấy JWT từ request gốc
                var token = GetJwtFromHeader();
                if (string.IsNullOrEmpty(token))
                {
                    return new UserApiResponse<Guid?>
                    {
                        ErrorMessage = "Missing JWT token",
                        Message = OperationResult.Error,
                    }; 
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("api/store", model);
                var raw = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("👉 Store API raw response ({StatusCode}): {Raw}",
                    (int)response.StatusCode, raw);

                // Nếu lỗi HTTP
                if (!response.IsSuccessStatusCode)
                {
                    return new UserApiResponse<Guid?>
                    {
                        Message = MapStatusCodeToResult((int)response.StatusCode),
                        ErrorMessage = $"Store API returned {(int)response.StatusCode} - {response.ReasonPhrase}",
                        Data = Guid.Empty
                    };
                }

                // Deserialize response về dạng StoreApiResponse
                var storeResponse = await response.Content.ReadFromJsonAsync<StoreApiResponse>();

                if (storeResponse == null)
                {
                    return new UserApiResponse<Guid?>
                    {
                        Message = OperationResult.Error,
                        ErrorMessage = "Invalid response from Store API",
                        Data = Guid.Empty
                    };
                        
                }

                return new UserApiResponse<Guid?>
                {
                    Message = MapStatusCodeToResult(storeResponse.StatusCode),
                    ErrorMessage = storeResponse.Message,
                    Data = storeResponse.Data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while calling Store API");
                return new UserApiResponse<Guid?>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = ("Exception while calling Store API"),
                    Data = Guid.Empty
                };
            } ;
            
        }

        /// <summary>
        /// Lấy JWT token từ header Authorization
        /// </summary>
        private string? GetJwtFromHeader()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            return string.IsNullOrWhiteSpace(authHeader) ? null : authHeader.Replace("Bearer ", "");
        }


    

        /// <summary>
        /// Map HTTP status code về OperationResult
        /// </summary>
        private static OperationResult MapStatusCodeToResult(int statusCode) => statusCode switch
        {
            >= 200 and < 300 => OperationResult.Success,
            400 => OperationResult.Failed, // nếu enum bạn có
            403 => OperationResult.Forbidden,
            404 => OperationResult.NotFound,
            409 => OperationResult.Conflict,
            >= 500 and < 600 => OperationResult.Error,
            _ => OperationResult.Error
        };
    }

    /// <summary>
    /// Model response thực tế từ Store API
    /// </summary>
    public class StoreApiResponse
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        [JsonPropertyName("data")]
        public Guid? Data { get; set; }
    }
}
