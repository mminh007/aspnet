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
        public Guid OrderId { get; set; }

        public Guid BuyerId { get; set; }

        public Guid StoreId { get; set; }

        public PaymentMethod Method { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "VND";
    }
}
