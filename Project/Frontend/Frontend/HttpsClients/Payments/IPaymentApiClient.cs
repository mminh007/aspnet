using Frontend.Models.Payments.Requests;
using static Frontend.Models.Payments.DTOs;

namespace Frontend.HttpsClients.Payments
{
    public interface IPaymentApiClient
    {
        Task<(bool Success, string? Message, int statusCode, PaymentDTO data)> CreatePayment(PaymentRequest request);
        Task<(bool Success, string? Message, int statusCode, PaymentDTO data)> ConfirmPayment(ConfirmPaymentRequest request);
    }
}
