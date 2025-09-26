using Payment.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.Common.Models.Responses
{
    public class PaymentResponseModel<T>
    {
        public bool Success { get; set; }
        public OperationResult Message { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Data { get; set; }
    }
}
