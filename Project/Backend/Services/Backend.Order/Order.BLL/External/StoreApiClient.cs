using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Order.BLL.External.Interfaces;
using Order.Common.Enums;
using Order.Common.Models;
using Order.Common.Models.Responses;
using Order.Common.Urls.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Order.BLL.External
{
    public class StoreApiClient : IStoreApiClient
    {
        private readonly ILogger<StoreApiClient> _logger;
        private readonly StoreEndpoints _endpoints;
        private readonly HttpClient _client;

        public StoreApiClient(ILogger<StoreApiClient> logger, IOptions<StoreEndpoints> endpoints, HttpClient client)
        {
            _logger = logger;
            _endpoints = endpoints.Value;
            _client = client;
        }
        public async Task<OrderResponseModel<DTOs.StoreDTO>> GetStoreInfoAsync(Guid storeId)
        {
            var url = _endpoints.GetStoreInfo.Replace("{storeId}", storeId.ToString());

            var response = await _client.GetAsync(url);

            return await ParseResponse<DTOs.StoreDTO>(response, "GetStoreInfo");
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
