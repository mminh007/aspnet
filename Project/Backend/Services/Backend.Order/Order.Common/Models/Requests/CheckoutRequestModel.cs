using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Common.Models.Requests
{
    public class CheckoutRequest
    {
        public List<Guid> ProductIds { get; set; } = new();
    }
}
