using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using User.BLL.External.Interfaces;
using User.Common.Enums;
using User.Common.Models.Responses;
using User.Common.Urls.Order;

namespace User.BLL.External
{
    public class OrderApiClient : IOrderApiClient
    {
        private readonly ILogger<OrderApiClient> _logger;
        private readonly OrderEndpoints _endpoints;
        private readonly HttpClient _httpClient;
        
        public OrderApiClient(ILogger<OrderApiClient> logger, HttpClient httpClient, IOptions<OrderEndpoints> endpoints)
        {
            _logger = logger;
            _httpClient = httpClient;
            _endpoints = endpoints.Value;
        }

        public async Task<UserApiResponse<int>> CreateCart(Guid userId)
        {
            var url = _endpoints.CreateCart.Replace("{userId}", userId.ToString());

            var response = await _httpClient.PostAsync(url, null);

            return await ParseResponse<int>(response, "CreateCart");
        }

        private async Task<UserApiResponse<T>> ParseResponse<T>(HttpResponseMessage response, string action)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<UserApiResponse<T>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result == null)
                {
                    return new UserApiResponse<T>
                    {
                        Message = OperationResult.Failed,
                        Data = default
                    };
                }

                return new UserApiResponse<T>
                {
                    Message = OperationResult.Success,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while parsing response for {Action}", action);
                return new UserApiResponse<T>
                {
                    Message = OperationResult.Error,
                    Data = default
                };
            }
        }

    }
}
