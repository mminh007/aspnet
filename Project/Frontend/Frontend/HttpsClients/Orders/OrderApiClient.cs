using Frontend.Cache.Interfaces;
using Frontend.HttpsClients.Products;
using Frontend.HttpsClients.Stores;
using Frontend.Models.Orders;
using Frontend.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.HttpsClients.Orders
{
    public class OrderApiClient : IOrderApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderApiClient> _logger;

        private readonly string _getCountingItems;
        private readonly string _addItemsToCart;
        public OrderApiClient(HttpClient HttpClient,
                              ILogger<OrderApiClient> logger,
                              IConfiguration config)
        {
            _httpClient = HttpClient;
            _logger = logger;
            _configuration = config;

            var endpoints = _configuration.GetSection("ServiceUrls:Order:Endpoints");
            _getCountingItems = endpoints["GetCountingItems"];
            _addItemsToCart = endpoints["AddItemsToCart"];
        }

        public Task<(bool Success, string? Message, int statusCode, Guid cartId)> AddItemsToCart(DTOs.AddItemToCartRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool Success, string? Message, int statusCode, DTOs.CountItemsDTO dto)> GetCountItemsToCart(Guid userId)
        {
            var url = _getCountingItems.Replace("{userId}", userId.ToString());
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonSerializer.Deserialize<OrderApiResponse<DTOs.CountItemsDTO>>(content,
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

        private class OrderApiResponse<T>
        {
            [JsonPropertyName("statusCode")] public int StatusCode { get; set; }
            [JsonPropertyName("message")] public string? Message { get; set; }
            [JsonPropertyName("data")] public T? Data { get; set; }
        }
    }
}
