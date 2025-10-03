using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Common.Models.Responses;

namespace User.BLL.External.Interfaces
{
    public interface IOrderApiClient
    {
        Task<UserApiResponse<int>> CreateCart(Guid userId);
    }
}
