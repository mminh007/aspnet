using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Common.Models.Requests
{
    public class UpdateStoreAmountRequest
    {
        public Guid StoreId { get; set; }
        public decimal Amount { get; set; }              // VND
        public string? IdempotencyKey { get; set; }      // dùng paymentIntent.Id
        public string? PaymentId { get; set; }
    }
}
