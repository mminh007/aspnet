using Frontend.Models.Stores;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.HttpsClients.Stores
{
    public class StoreApiClient : IStoreApiClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;

        private static string _getAllStore;
        private static string _getStoreDetail;
        public StoreApiClient(HttpClient client, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger logger )
        {
            _client = client;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            var endpoint = _configuration.GetSection("ServiceUrl:Store:Endpoints");
            _getAllStore = endpoint["GetAllStore"];
            _getStoreDetail = endpoint["GetStoreDetail"];
        }

        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<StoreDto?> Data)> GetAllStoresActiveAsync()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("❌ No JWT token found in request headers");
                return (false, "No JWT token found in request headers", 401, null);

            }

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync(_getAllStore);

            var content = await response.Content.ReadAsStringAsync();


            try
            {
                var result = JsonSerializer.Deserialize<StoreApiResponse<IEnumerable<StoreDto>>>(content,
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
