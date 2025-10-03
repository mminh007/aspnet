using Payment.Common.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.BLL.External.Interface
{
    public interface IAuthApiClient
    {
        Task<PaymentResponseModel<string>> GetSystemToken(Guid id, string status);
    }
}
