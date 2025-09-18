using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Order.BLL.External.Interfaces;
using Order.Common.Enums;
using Order.Common.Models; // DTOs.CartProductDTO
using Order.Common.Models.Responses;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Order.BLL.External
{
    public class ProductApiClient : IProductApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductApiClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public ProductApiClient(HttpClient httpClient, ILogger<ProductApiClient> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        /// <summary>
        /// Lấy thông tin product cho Order (giá, số lượng, trạng thái)
        /// </summary>
        public async Task<OrderResponseModel<List<DTOs.CartProductDTO>>> GetProductInfoAsync(List<Guid> productId)
        {
            try
            {
                // ✅ Lấy JWT từ request gốc
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
                    .ToString()
                    .Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("❌ No JWT token found in request headers");
                    return new OrderResponseModel<List<DTOs.CartProductDTO>>
                    {
                        Success = false,
                        Message = OperationResult.Error,
                        ErrorMessage = "Missing JWT token"
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var baseUrl = _configuration["ServiceUrls:Product:BaseUrl"];
                var endpoint = _configuration["ServiceUrls:Product:Endpoints:GetProductInfo"];
                var url = $"{baseUrl}/{endpoint}";

                // ✅ Gọi sang Product API
                var response = await _httpClient.PostAsJsonAsync(url, productId);

                var raw = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("👉 Product API raw response ({StatusCode}): {Raw}",
                    (int)response.StatusCode, raw);

                if (!response.IsSuccessStatusCode)
                {
                    return new OrderResponseModel<List<DTOs.CartProductDTO>>
                    {
                        Success = false,
                        Message = MapStatusCodeToResult((int)response.StatusCode),
                        ErrorMessage = $"Product API returned {(int)response.StatusCode} - {response.ReasonPhrase}"
                    };
                }

                var productResponse = await response.Content.ReadFromJsonAsync<ExternalResponse<List<DTOs.CartProductDTO>>>();

                if (productResponse == null)
                {
                    _logger.LogError("❌ Failed to parse Product API response");
                    return new OrderResponseModel<List<DTOs.CartProductDTO>>
                    {
                        Success = false,
                        Message = OperationResult.Error,
                        ErrorMessage = "Failed to parse Product API response"
                    };
                }

                _logger.LogInformation($"productResponse Data: {productResponse.Data}");

                return new OrderResponseModel<List<DTOs.CartProductDTO>>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = productResponse.Data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while calling Product API");

                return new OrderResponseModel<List<DTOs.CartProductDTO>>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    ErrorMessage = "Exception while calling Product API"
                };
            }
        }

        private OperationResult MapStatusCodeToResult(int statusCode) => statusCode switch
        {
            200 => OperationResult.Success,
            400 => OperationResult.Failed,
            401 => OperationResult.Error,
            403 => OperationResult.Forbidden,
            404 => OperationResult.NotFound,
            409 => OperationResult.Conflict,
            _ => OperationResult.Error
        };

        private class ExternalResponse<T>
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
