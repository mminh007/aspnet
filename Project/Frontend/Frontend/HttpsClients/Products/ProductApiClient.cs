using Frontend.Models.Products;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.HttpsClients.Products
{
    public class ProductApiClient : IProductApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductApiClient> _logger;

        private readonly string _getByStoreEndpoint;
        private readonly string _getByIdEndpoint;
        private readonly string _searchCategoriesEndpoint;

        public ProductApiClient(HttpClient httpClient,
                                IConfiguration configuration,
                                ILogger<ProductApiClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            var endpoints = _configuration.GetSection("ServiceUrls:Product:Endpoints");
            _getByStoreEndpoint = endpoints["GetByStore"];
            _getByIdEndpoint = endpoints["GetById"];
            _searchCategoriesEndpoint = endpoints["SearchCategories"];
        }

        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.ProductBuyerDTO>? Data)> GetByStoreAsync(Guid storeId)
        {
            var url = _getByStoreEndpoint.Replace("{storeId}", storeId.ToString());
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonSerializer.Deserialize<ProductApiResponse<IEnumerable<DTOs.ProductBuyerDTO>>>(content,
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

        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<DTOs.CategoryDTO>? Data)> SearchCategoriesAsync(Guid storeId)
        {
            var url = _searchCategoriesEndpoint.Replace("{storeId}", storeId.ToString());
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonSerializer.Deserialize<ProductApiResponse<IEnumerable<DTOs.CategoryDTO>>>(content,
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
      
        private class ProductApiResponse<T>
        {
            [JsonPropertyName("statusCode")] public int StatusCode { get; set; }
            [JsonPropertyName("message")] public string? Message { get; set; }
            [JsonPropertyName("data")] public T? Data { get; set; }
        }
    }
}


//public async Task<(bool Success, string? Message, int statusCode, DTOs.ProductBuyerDTO? Data)> GetByIdAsync(Guid productId)
//{
//    var url = _getByIdEndpoint.Replace("{productId}", productId.ToString());
//    var response = await _httpClient.GetAsync(url);
//    var content = await response.Content.ReadAsStringAsync();

//    try
//    {
//        var result = JsonSerializer.Deserialize<ProductApiResponse<DTOs.ProductBuyerDTO>>(content,
//            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

//        if (!response.IsSuccessStatusCode || result == null)
//        {
//            return (false, result?.Message ?? $"Request failed: {content}", (int)response.StatusCode, null);
//        }

//        return (true, result.Message, result.StatusCode, result.Data);
//    }
//    catch (Exception ex)
//    {
//        _logger.LogError(ex, "❌ Exception while parsing response");
//        return (false, $"Exception while parsing response: {ex.Message}", (int)response.StatusCode, null);
//    }
//}
