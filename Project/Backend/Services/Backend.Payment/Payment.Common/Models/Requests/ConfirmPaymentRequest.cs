using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Common.Models.Requests
{
    public class ConfirmPaymentRequest
    {
        [Required]
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}
