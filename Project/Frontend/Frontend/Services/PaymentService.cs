using Frontend.HttpsClients.Payments;
using Frontend.Models.Payments;
using Frontend.Models.Payments.Requests;
using Frontend.Services.Interfaces;
using System.Runtime.CompilerServices;

namespace Frontend.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentApiClient _httpClient;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IPaymentApiClient ApiClient, ILogger<PaymentService> logger)
        {
            _httpClient = ApiClient;
            _logger = logger;
        }

        public async Task<(string Message, int StatusCode, DTOs.PaymentDTO data)> ConfirmPaymentAsync(ConfirmPaymentRequest request)
        {
            var result = await _httpClient.ConfirmPayment(request);

            return (result.Message, result.statusCode, result.data);
        }

        public async Task<(string Message, int StatusCode, DTOs.PaymentDTO data)> CreatePaymentAsync(PaymentRequest request)
        {
            var result = await _httpClient.CreatePayment(request);
            return (result.Message, result.statusCode, result.data);
        }
    }
}
