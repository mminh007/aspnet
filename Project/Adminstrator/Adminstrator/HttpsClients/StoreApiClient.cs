using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Models.Stores;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Adminstrator.HttpsClients
{
    public class StoreApiClient : IStoreApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<StoreApiClient> _logger; //= LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<StoreApiClient>();

        private readonly string _getByUserIdEndpoint;
        private readonly string _getByIdEndpoint;

        public StoreApiClient(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<StoreApiClient> logger )
        {
            _httpClient = httpClient;
            _configuration = configuration;


            var endpoints = _configuration.GetSection("ServiceUrls:Store:Endpoints");
            _getByUserIdEndpoint = endpoints["GetByUserId"];
            _getByIdEndpoint = endpoints["GetById"];

            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> GetByUserIdAsync(Guid userId)
        {

            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("❌ No JWT token found in request headers");
                return (false, "No JWT token found in request headers", 401, null);

            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var url = _getByUserIdEndpoint.Replace("{userId}", userId.ToString());
            var response = await _httpClient.GetAsync(url);

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonSerializer.Deserialize<StoreApiResponse<StoreDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result == null)
                {
                    return (false, result?.Message ?? $"Request failed: {content}", (int)response.StatusCode, null);
                }

                return (true, result.Message, result.StatusCode, result.Data);
            }
            catch (Exception ex)
            {
                return (false, $"Exception while parsing response: {ex.Message}", (int)response.StatusCode, null);
            }
        }


        public async Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> GetByIdAsync(Guid storeId)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("❌ No JWT token found in request headers");
                return (false, "No JWT token found in request headers", 401, null);
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var url = _getByIdEndpoint.Replace("{storeId}", storeId.ToString());
            var response = await _httpClient.GetAsync(url);

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonSerializer.Deserialize<StoreApiResponse<StoreDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result == null)
                {
                    return (false, result?.Message ?? $"Request failed: {content}", (int)response.StatusCode, null);
                }

                return (true, result.Message, result.StatusCode, result.Data);
            }
            catch (Exception ex)
            {
                return (false, $"Exception while parsing response: {ex.Message}", (int)response.StatusCode, null);
            }
        }



        // ✅ Private generic response wrapper
        private class StoreApiResponse<T>
        {
            [JsonPropertyName("statusCode")]
            public int StatusCode { get; set; }

            [JsonPropertyName("message")]
            public string? Message { get; set; }

            [JsonPropertyName("data")]
            public T? Data { get; set; }
        }
    }
}
