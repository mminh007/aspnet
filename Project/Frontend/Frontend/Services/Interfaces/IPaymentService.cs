using Frontend.Models.Payments.Requests;
using static Frontend.Models.Payments.DTOs;


namespace Frontend.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<(string Message, int StatusCode, PaymentDTO data)> CreatePaymentAsync(PaymentRequest request);

        Task<(string Message, int StatusCode, PaymentDTO data )> ConfirmPaymentAsync(ConfirmPaymentRequest request);
    }
}
