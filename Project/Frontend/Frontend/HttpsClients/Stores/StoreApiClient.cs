using Frontend.Models.Stores;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.HttpsClients.Stores
{
    public class StoreApiClient : IStoreApiClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StoreApiClient> _logger;

        private static string _getAllStore;
        private static string _getStoreDetail;

        public StoreApiClient(HttpClient client,
                              IConfiguration configuration,
                              ILogger<StoreApiClient> logger)
        {
            _client = client;
            _configuration = configuration;
            _logger = logger;

            var endpoint = _configuration.GetSection("ServiceUrls:Store:Endpoints");
            _getAllStore = endpoint["GetAllStore"];
            _getStoreDetail = endpoint["GetStoreDetail"];
        }

        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<StoreDto?> Data)> GetStoresPagedAsync(int page, int pageSize)
        {
            var url = _getAllStore
                .Replace("{page}", page.ToString())
                .Replace("{pageSize}", pageSize.ToString());

            var response = await _client.GetAsync(url);
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
                _logger.LogError(ex, "❌ Exception while parsing response");
                return (false, $"Exception while parsing response: {ex.Message}", (int)response.StatusCode, null);
            }
        }

        public async Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> GetStoreByIdAsync(Guid storeId)
        {

            var url = _getStoreDetail.Replace("{storeId}", storeId.ToString());
            var response = await _client.GetAsync(url);

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
        private class StoreApiResponse<T>
        {
            [JsonPropertyName("statusCode")] public int StatusCode { get; set; }
            [JsonPropertyName("message")] public string? Message { get; set; }
            [JsonPropertyName("data")] public T? Data { get; set; }
        }
    }
}
