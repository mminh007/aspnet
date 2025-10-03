using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.BLL.External.Interface;
using Payment.Common.Enums;
using Payment.Common.Models.Responses;
using Payment.Common.Urls.Auth;
using Payment.Common.Urls.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Payment.BLL.External
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

        public async Task<PaymentResponseModel<string>> GetSystemToken(Guid id, string status)
        {
            var url = _endpoints.GetSystemToken;

            var response = await _client.GetAsync(url);

            return await ParseResponse<string>(response, "GetSystemToken");
        }

        private async Task<PaymentResponseModel<T>> ParseResponse<T>(HttpResponseMessage response, string action)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<InternalServiceResponse<T>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result == null)
                {
                    return new PaymentResponseModel<T>
                    {
                        Success = false,
                        Message = OperationResult.Failed,
                        Data = default
                    };
                }

                return new PaymentResponseModel<T>
                {
                    Success = true,
                    Message = OperationResult.Success,
                    Data = result.Data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while parsing response for {Action}", action);
                return new PaymentResponseModel<T>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    Data = default
                };
            }
        }
    }
}
