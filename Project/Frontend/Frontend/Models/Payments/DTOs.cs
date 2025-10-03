using Frontend.Enums;

namespace Frontend.Models.Payments
{
    public class DTOs
    {
        public class PaymentDTO
        {
            public Guid PaymentId { get; set; }
            public Guid OrderId { get; set; }
            public PaymentMethod Method { get; set; }
            public PaymentStatus Status { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
            public string? TransactionId { get; set; }
            public DateTime CreatedAt { get; set; }
            public string? ClientSecret { get; set; }

        }

        public class PaymentRequestDTO
        {
            public string OrderId { get; set; }  // để client gửi string
            public string BuyerId { get; set; }
            public string Method { get; set; }   // string, không enum
            public decimal Amount { get; set; }
            public string Currency { get; set; }
        }
    }
}
