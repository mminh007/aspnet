using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Order.BLL.External.Interfaces;
using Order.Common.Enums;
using Order.Common.Models; // DTOs.CartProductDTO
using Order.Common.Models.Responses;
using Order.Common.Urls.Product;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Order.BLL.External
{
    public class ProductApiClient : IProductApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductApiClient> _logger;
        private readonly ProductEndpoints _endpoints;

        public ProductApiClient(ILogger<ProductApiClient> logger, IOptions<ProductEndpoints> endpoints, HttpClient client)
        {
            _logger = logger;
            _endpoints = endpoints.Value;
            _httpClient = client;
        }

        /// <summary>
        /// Lấy thông tin product cho Order (giá, số lượng, trạng thái)
        /// </summary>
        /// 
        public async Task<OrderResponseModel<List<DTOs.CartProductDTO>>> GetProductInfoAsync(List<Guid> productIdList)
        {
            var url = _endpoints.GetProductInfo;
            var response = await _httpClient.PostAsJsonAsync(url, productIdList);

            return await ParseResponse<List<DTOs.CartProductDTO>>(response, "GetProductInfo");
        }

        public async Task<OrderResponseModel<DTOs.CartProductDTO>> ValidateProduct(Guid productId)
        {
            var url = _endpoints.ValidateProduct.Replace("{productId}", productId.ToString());

            var response = await _httpClient.PostAsJsonAsync(url, productId);

            return await ParseResponse<DTOs.CartProductDTO>(response, "ValidateProduct");
        }

        private async Task<OrderResponseModel<T>> ParseResponse<T>(HttpResponseMessage response, string action)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<InternalServiceResponse<T>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result == null)
                {
                    return new OrderResponseModel<T>
                    {
                        Success = false,
                        Message = OperationResult.Failed,
                        Data = default
                    };
                }

                return new OrderResponseModel<T>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while parsing response for {Action}", action);
                return new OrderResponseModel<T>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    Data = default
                };
            }
        }

    }
}
