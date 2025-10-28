using Frontend.Enums;
using System.ComponentModel.DataAnnotations;

namespace Frontend.Models.Payments.Requests
{
    public class PaymentRequest
    {
        [Required]
        public Guid OrderId { get; set; }

        public Guid BuyerId { get; set; }

        public Guid StoreId { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "VND";
    }
}
