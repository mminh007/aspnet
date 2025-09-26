using Payment.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Common.Models
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
    }
}
