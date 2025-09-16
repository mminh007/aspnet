using Order.Common.Models;
using Order.Common.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.BLL.External.Interfaces
{
    public interface IProductApiClient
    {
        Task<OrderResponseModel<DTOs.CartProductDTO>> GetProductInfoAsync(Guid productId);
    }
}
