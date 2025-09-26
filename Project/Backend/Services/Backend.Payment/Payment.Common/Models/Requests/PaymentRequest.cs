using Payment.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Common.Models.Requests
{
    public class PaymentRequest
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid BuyerId { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "VND";
    }
}
