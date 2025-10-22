using Order.Common.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.BLL.External.Interfaces
{
    public interface IAuthApiClient
    {
        Task<OrderResponseModel<string>> GetSystemToken(Guid id, string status);
    }
}
