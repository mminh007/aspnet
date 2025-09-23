using Frontend.Cache.Interfaces;
using Frontend.HttpsClients.Products;
using Frontend.HttpsClients.Stores;
using Frontend.Models.Orders;
using Frontend.Models.Orders.Requests;
using Frontend.Services;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.HttpsClients.Orders
{
    public class OrderApiClient : IOrderApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderApiClient> _logger;

        private readonly string _getCart;
        private readonly string _getCountingItems;
        private readonly string _addItemsToCart;
        private readonly string _updateItemQuantity;
        private readonly string _removeItem;
        private readonly string _clearCart;

        public OrderApiClient(HttpClient httpClient, ILogger<OrderApiClient> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = config;

            var endpoints = _configuration.GetSection("ServiceUrls:Order:Endpoints");
            _getCart = endpoints["GetCart"];
            _getCountingItems = endpoints["GetCountingItems"];
            _addItemsToCart = endpoints["AddItemsToCart"];
            _updateItemQuantity = endpoints["UpdateItemQuantity"];
            _removeItem = endpoints["RemoveItem"];
            _clearCart = endpoints["ClearCart"];
        }

        public async Task<(bool Success, string? Message, int statusCode, DTOs.CartDTO data)> GetCart(Guid userId)
        {
            var url = _getCart.Replace("{userId}", userId.ToString());
            var response = await _httpClient.GetAsync(url);
            return await ParseResponse<DTOs.CartDTO>(response, "GetCart");
        }

        public async Task<(bool Success, string? Message, int statusCode, DTOs.CountItemsDTO data)> GetCountItemsToCart(Guid userId)
        {
            var url = _getCountingItems.Replace("{userId}", userId.ToString());
            var response = await _httpClient.GetAsync(url);
            return await ParseResponse<DTOs.CountItemsDTO>(response, "GetCountItemsToCart");
        }

        public async Task<(bool Success, string? Message, int statusCode, int TotalItems)> AddItemsToCart(Guid userId, RequestItemsToCartModel request)
        {
            var url = _addItemsToCart.Replace("{userId}", userId.ToString());
            var response = await _httpClient.PostAsJsonAsync(url, request);
            var parsed = await ParseResponse<int>(response, "AddItemsToCart");
            return (parsed.Success, parsed.Message, parsed.statusCode, parsed.dto);
        }

        public async Task<(bool Success, string? Message, int statusCode, DTOs.CartDTO data)> UpdateItemQuantity(Guid userId, Guid cartItemId, UpdateQuantityModel request)
        {
            var url = _updateItemQuantity.Replace("{userId}", userId.ToString())
                                         .Replace("{cartItemId}", cartItemId.ToString());
            var response = await _httpClient.PutAsJsonAsync(url, request);
            var parsed = await ParseResponse<DTOs.CartDTO>(response, "UpdateItemQuantity");
            return (parsed.Success, parsed.Message, parsed.statusCode, parsed.dto);
        }

        public async Task<(bool Success, string? Message, int statusCode)> RemoveItem(Guid userId, Guid productId)
        {
            var url = _removeItem.Replace("{userId}", userId.ToString())
                                 .Replace("{productId}", productId.ToString());
            var response = await _httpClient.DeleteAsync(url);
            var parsed = await ParseResponse<object>(response, "RemoveItem");
            return (parsed.Success, parsed.Message, parsed.statusCode);
        }

        public async Task<(bool Success, string? Message, int statusCode)> ClearCart(Guid userId)
        {
            var url = _clearCart.Replace("{userId}", userId.ToString());
            var response = await _httpClient.DeleteAsync(url);
            var parsed = await ParseResponse<object>(response, "ClearCart");
            return (parsed.Success, parsed.Message, parsed.statusCode);
        }

        private async Task<(bool Success, string? Message, int statusCode, T dto)> ParseResponse<T>(HttpResponseMessage response, string action)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<OrderApiResponse<T>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result == null)
                {
                    return (false, result?.Message ?? $"[{action}] Request failed: {content}", (int)response.StatusCode, default);
                }

                return (true, result.Message, result.StatusCode, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Exception while parsing response for {action}");
                return (false, $"Exception: {ex.Message}", (int)response.StatusCode, default);
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
