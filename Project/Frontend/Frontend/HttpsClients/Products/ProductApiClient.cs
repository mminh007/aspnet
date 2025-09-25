using Frontend.Configs;
using Frontend.Configs.Product;
using Frontend.Models.Products;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.HttpsClients.Products
{
    public class ProductApiClient : IProductApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductApiClient> _logger;
        private readonly ProductEndpoints _endpoints;

        public ProductApiClient(HttpClient httpClient,
                                ILogger<ProductApiClient> logger,
                                IOptions<ProductEndpoints> endpoints)
        {
            _httpClient = httpClient;
            _logger = logger;
            _endpoints = endpoints.Value;
        }

        // ----------- GetByStore -----------
        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.ProductBuyerDTO>? Data)>
            GetByStoreAsync(Guid storeId)
        {
            var url = _endpoints.GetByStore.Replace("{storeId}", storeId.ToString());
            var response = await _httpClient.GetAsync(url);
            return await ParseResponse<IEnumerable<DTOs.ProductBuyerDTO>>(response, "GetByStore");
        }

        // ----------- GetById -----------
        public async Task<(bool Success, string? Message, int statusCode, DTOs.ProductBuyerDTO? Data)>
            GetByIdAsync(Guid productId)
        {
            var url = _endpoints.GetById.Replace("{productId}", productId.ToString());
            var response = await _httpClient.GetAsync(url);
            return await ParseResponse<DTOs.ProductBuyerDTO>(response, "GetById");
        }

        // ----------- SearchCategories -----------
        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.CategoryDTO>? Data)>
            SearchCategoriesAsync(Guid storeId)
        {
            var url = _endpoints.SearchCategories.Replace("{storeId}", storeId.ToString());
            var response = await _httpClient.GetAsync(url);
            return await ParseResponse<IEnumerable<DTOs.CategoryDTO>>(response, "SearchCategories");
        }

        // ----------- Common Parse ----------- 
        private async Task<(bool Success, string? Message, int statusCode, T? Data)>
            ParseResponse<T>(HttpResponseMessage response, string action)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<ProductApiResponse<T>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result == null)
                {
                    return (false, result?.Message ?? $"❌ {action} failed: {content}", (int)response.StatusCode, default);
                }

                return (true, result.Message, result.StatusCode, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while parsing response for {Action}", action);
                return (false, $"Exception while parsing response: {ex.Message}", (int)response.StatusCode, default);
            }
        }

        private class ProductApiResponse<T>
        {
            [JsonPropertyName("statusCode")] public int StatusCode { get; set; }
            [JsonPropertyName("message")] public string? Message { get; set; }
            [JsonPropertyName("data")] public T? Data { get; set; }
        }
    }
}
