using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Common.Models.Requests
{
    public class DeleteOrderRequest
    {
        public Guid OrderId { get; set; }
    }
}
