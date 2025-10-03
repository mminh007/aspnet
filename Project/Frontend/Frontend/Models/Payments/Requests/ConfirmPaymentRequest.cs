using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Payments.Requests
{
    public class ConfirmPaymentRequest
    {
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}
