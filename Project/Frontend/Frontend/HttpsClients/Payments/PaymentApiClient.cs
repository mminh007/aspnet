using Frontend.Configs.Payment;
using Frontend.Configs.Product;
using Frontend.Models.Auth;
using Frontend.Models.Payments.Requests;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Frontend.Models.Payments.DTOs;

namespace Frontend.HttpsClients.Payments
{
 
    public class PaymentApiClient : IPaymentApiClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<PaymentApiClient> _logger;
        private readonly PaymentEndpoints _endpoints;

        public PaymentApiClient(HttpClient client, ILogger<PaymentApiClient> logger, IOptions<PaymentEndpoints> endpoints)
        {
            _client = client;
            _logger = logger;
            _endpoints = endpoints.Value;
        }

        public async Task<(bool Success, string? Message, int statusCode, PaymentDTO data)> ConfirmPayment(ConfirmPaymentRequest request)
        {
            var url = _endpoints.Confirm;
            var response = await _client.PostAsJsonAsync(url, request);

            return await ParseResponse<PaymentDTO>(response, "confirmPayment");
        }

        public async Task<(bool Success, string? Message, int statusCode, PaymentDTO data)> CreatePayment(PaymentRequest request)
        {
            var url = _endpoints.Create;

            _logger.LogInformation("Sending payment request: {@Request}", request);
            var jsonContent = JsonSerializer.Serialize(request);
            _logger.LogInformation("JSON payload: {Json}", jsonContent);

            var response = await _client.PostAsJsonAsync(url, request);

            return await ParseResponse<PaymentDTO>(response, "CreatePayment");

        }

        private async Task<(bool Success, string? Message, int statusCode, T? Data)>
            ParseResponse<T>(HttpResponseMessage response, string action)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<PaymentApiResponse<T>>(content,
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

        private class PaymentApiResponse<T>
        {
            [JsonPropertyName("statusCode")] public int StatusCode { get; set; }
            [JsonPropertyName("message")] public string? Message { get; set; }
            [JsonPropertyName("data")] public T? Data { get; set; }
        }
    }
}
