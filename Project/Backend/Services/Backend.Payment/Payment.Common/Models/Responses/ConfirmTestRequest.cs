using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Common.Models.Responses
{
    public class ConfirmTestRequest
    {
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}
