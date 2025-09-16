using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Common.Models.Requests
{
    public class RequestItemToCartModel
    {
        public Guid StoreId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
