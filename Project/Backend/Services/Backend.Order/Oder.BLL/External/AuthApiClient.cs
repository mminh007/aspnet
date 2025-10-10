using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Order.BLL.External.Interfaces;
using Order.Common.Enums;
using Order.Common.Models.Responses;
using Order.Common.Urls.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Order.BLL.External
{
    public class AuthApiClient : IAuthApiClient
    {
        private readonly ILogger<AuthApiClient> _logger;
        private readonly AuthEndpoints _endpoints;
        private readonly HttpClient _client;

        public AuthApiClient(ILogger<AuthApiClient> logger, IOptions<AuthEndpoints> endpoints, HttpClient client)
        {
            _logger = logger;
            _endpoints = endpoints.Value;
            _client = client;
        }

        public async Task<OrderResponseModel<string>> GetSystemToken(Guid id, string status)
        {
            var url = _endpoints.GetSystemToken;

            var response = await _client.GetAsync(url);

            return await ParseResponse<string>(response, "GetSystemToken");
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
