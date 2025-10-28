using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Payment.BLL.External.Interface;
using Payment.Common.Enums;
using Payment.Common.Models.Requests;
using Payment.Common.Models.Responses;
using Payment.Common.Urls.Store;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Payment.BLL.External
{
    public sealed class StoreApiClient : IStoreApiClient
    {
        private readonly HttpClient _http;
        private readonly StoreEndpoints _endpoints;
        private readonly ILogger<StoreApiClient> _logger;

        // Retry đơn giản: 3 lần, backoff 200/500/1000ms
        private static AsyncRetryPolicy<HttpResponseMessage> RetryPolicy =>
            Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(r => (int)r.StatusCode >= 500)
                .WaitAndRetryAsync(new[]
                {
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromMilliseconds(1000)
                });

        public StoreApiClient(HttpClient http, ILogger<StoreApiClient> logger, IOptions<StoreEndpoints> endpoints)
        {
            _http = http;
            _endpoints = endpoints.Value;
            _logger = logger;
        }

        public async Task<PaymentResponseModel<string>> CreditAsync(StoreSettleRequest req, CancellationToken ct = default)
        {
            var url = _endpoints.Settlements;
            using var message = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(req)
            };

            // Idempotency để Store bỏ qua trùng lặp
            if (!string.IsNullOrWhiteSpace(req.IdempotencyKey))
                message.Headers.TryAddWithoutValidation("Idempotency-Key", req.IdempotencyKey);

            HttpResponseMessage resp;
            try
            {
                resp = await _http.SendAsync(message, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ StoreApiClient.CreditAsync send failed");
                return new PaymentResponseModel<string>
                {
                    Success = false,
                    Message = OperationResult.Error,
                    Data = default
                };
            }

            return await ParseResponse<string>(resp, "Store.Settlements");
        }

        private async Task<PaymentResponseModel<T>> ParseResponse<T>(HttpResponseMessage response, string action)
        {
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonSerializer.Deserialize<InternalServiceResponse<T>>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result == null)
                {
                    _logger.LogWarning("⚠️ StoreApiClient {Action} failed. Status={Status} Body={Body}",
                        action, (int)response.StatusCode, content);

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
                _logger.LogError(ex, "❌ Exception while parsing StoreApiClient response for {Action}. Body={Body}", action, content);
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
