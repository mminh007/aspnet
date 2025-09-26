
using Payment.Common.Enums;
using Payment.Common.Models.Requests;
using Payment.Common.Models.Responses;
using static Payment.Common.Models.DTOs;


namespace Payment.BLL.Services.Interfaces
{
    public interface IPaymentService
    {
        // Tạo payment intent (Stripe)
        Task<PaymentResponseModel<PaymentDTO>> CreatePaymentAsync(PaymentRequest request);

        // Xác nhận payment (sau khi client trả về paymentIntentId / token từ Stripe)
        Task<PaymentResponseModel<PaymentDTO>> ConfirmPaymentAsync(ConfirmPaymentRequest request);

        // Lấy chi tiết payment
        Task<PaymentResponseModel<PaymentDTO>> GetPaymentByIdAsync(Guid paymentId);

        // Lấy payment theo Order
        Task<PaymentResponseModel<PaymentDTO>> GetPaymentByOrderIdAsync(Guid orderId);

        // Lấy danh sách payment theo User
        Task<PaymentResponseModel<IEnumerable<PaymentDTO>>> GetPaymentsByUserAsync(Guid userId);

        Task<PaymentResponseModel<bool>> UpdatePaymentStatusAsync(string paymentIntentId, PaymentStatus status, string? message = null);

        // Hủy payment
        Task<PaymentResponseModel<bool>> CancelPaymentAsync(Guid paymentId);
    }
}
