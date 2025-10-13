using Frontend.Configs;
using Frontend.Configs.Order;
using Frontend.Models.Orders.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Frontend.Models.Orders.DTOs;

namespace Frontend.HttpsClients.Orders
{
    public class OrderApiClient : IOrderApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderApiClient> _logger;
        private readonly OrderEndpoints _endpoints;

        public OrderApiClient(HttpClient httpClient, ILogger<OrderApiClient> logger, IOptions<OrderEndpoints> endpoints)
        {
            _httpClient = httpClient;
            _logger = logger;
            _endpoints = endpoints.Value;
        }

        // -------- CART METHODS --------
        public async Task<(bool Success, string? Message, int statusCode, CartDTO data)> GetCart(Guid userId)
        {
            var url = _endpoints.GetCart;
            var response = await _httpClient.GetAsync(url);
            return await ParseResponse<CartDTO>(response, "GetCart");
        }

        public async Task<(bool Success, string? Message, int statusCode, CountItemsDTO data)> GetCountItemsToCart(Guid userId)
        {
            var url = _endpoints.GetCountingItems;
            var response = await _httpClient.GetAsync(url);
            return await ParseResponse<CountItemsDTO>(response, "GetCountingItems");
        }

        public async Task<(bool Success, string? Message, int statusCode, CartDTO data)> AddItemsToCart(Guid userId, RequestItemsToCartModel dto)
        {
            var url = _endpoints.AddItemsToCart;
            var response = await _httpClient.PostAsJsonAsync(url, dto);
            return await ParseResponse<CartDTO>(response, "AddItemsToCart");
        }

        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<CartItemDTO> data)> GetCartItemInStore(Guid storeId)
        {
            var url = _endpoints.GetCartInStore.Replace("{storeId}", storeId.ToString());
            var response = await _httpClient.GetAsync(url);

            return await ParseResponse<IEnumerable<CartItemDTO>>(response, "GetCartItemInStore");
        }


        // -------- ORDER METHODS --------
        public async Task<(bool Success, string? Message, int statusCode, OrderDTO data)> GetOrderById(Guid orderId)
        {
            var url = _endpoints.GetOrderById.Replace("{orderId}", orderId.ToString());
            var response = await _httpClient.GetAsync(url);
            return await ParseResponse<OrderDTO>(response, "GetOrderById");
        }

        public async Task<(bool Success, string? Message, int statusCode, List<OrderDTO> data)> GetOrdersByUser(Guid userId)
        {
            var url = _endpoints.GetOrdersByUser;
            var response = await _httpClient.GetAsync(url);
            return await ParseResponse<List<OrderDTO>>(response, "GetOrdersByUser");
        }

        public async Task<(bool Success, string? Message, int statusCode)> DeleteOrder(Guid orderId)
        {
            var url = _endpoints.DeleteOrder.Replace("{orderId}", orderId.ToString());
            var response = await _httpClient.DeleteAsync(url);
            var parsed = await ParseResponse<object>(response, "DeleteOrder");
            return (parsed.Success, parsed.Message, parsed.statusCode);
        }

        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<OrderDTO> data)> CreateOrder(Guid userId, IEnumerable<Guid> productIds)
        {
            var url = _endpoints.CreateOrder;
            var response = await _httpClient.PostAsJsonAsync(url, productIds);
            return await ParseResponse<List<OrderDTO>>(response, "CreateOrder");
        }    


        public async Task<(bool Success, string? Message, int statusCode, CartDTO data)> UpdateItemQuantity(Guid cartItemId, UpdateQuantityModel request)
        {
            var url = _endpoints.UpdateItemQuantity.Replace("{cartItemId}", cartItemId.ToString());

            var response = await _httpClient.PutAsJsonAsync(url, request);
            var parsed = await ParseResponse<CartDTO>(response, "UpdateItemQuantity");
            return (parsed.Success, parsed.Message, parsed.statusCode, parsed.dto);
        }

        public async Task<(bool Success, string? Message, int statusCode, CartDTO data)> RemoveItem(Guid userId, Guid cartItemId)
        {
            var url = _endpoints.RemoveItem.Replace("{cartItemId}", cartItemId.ToString());

            var response = await _httpClient.DeleteAsync(url);
            var parsed = await ParseResponse<CartDTO>(response, "RemoveItem");
            return (parsed.Success, parsed.Message, parsed.statusCode, parsed.dto);
        }

        public async Task<(bool Success, string? Message, int statusCode)> ClearCart(Guid userId)
        {
            var url = _endpoints.ClearCart;
            var response = await _httpClient.DeleteAsync(url);
            var parsed = await ParseResponse<object>(response, "ClearCart");
            return (parsed.Success, parsed.Message, parsed.statusCode);
        }

       

        // -------- ParseResponse (giữ nguyên logic cũ) --------
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
