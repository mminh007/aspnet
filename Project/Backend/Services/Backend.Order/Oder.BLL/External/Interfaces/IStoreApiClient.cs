using Order.Common.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Order.Common.Models.DTOs;

namespace Order.BLL.External.Interfaces
{
    public interface IStoreApiClient
    {
        Task<OrderResponseModel<StoreDTO>> GetStoreInfoAsync(Guid storeId);

    }
}
