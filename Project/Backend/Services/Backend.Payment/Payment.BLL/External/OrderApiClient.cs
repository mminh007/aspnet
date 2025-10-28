using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.BLL.External.Interface;
using Payment.Common.Models.Responses;
using Payment.Common.Urls.Order;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Payment.Common.Enums;

namespace Payment.BLL.External
{
    public class OrderApiClient : IOrderApiClient
    {
        private readonly ILogger<OrderApiClient> _logger;
        private readonly OrderEndpoints _endpoints;
        private readonly HttpClient _client;

        public OrderApiClient(ILogger<OrderApiClient> logger, IOptions<OrderEndpoints> endpoints, HttpClient client)
        {
            _logger = logger;
            _endpoints = endpoints.Value;
            _client = client;
        }

        public async Task<PaymentResponseModel<string>> UpdateStatusOrder(Guid orderId, string status, decimal t)
        {
            var url = _endpoints.UpdateStatus.Replace("{orderId}", orderId.ToString())
                                                  .Replace("{char}", status);
            var amount = JsonContent.Create(t);
            var response = await _client.PutAsync(url, amount);

            return await ParseResponse<string>(response, "UpdateStatus");
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
