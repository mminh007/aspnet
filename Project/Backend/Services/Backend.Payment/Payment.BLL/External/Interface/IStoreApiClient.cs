using Payment.Common.Models.Requests;
using Payment.Common.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payment.BLL.External.Interface
{
    public interface IStoreApiClient
    {
        Task<PaymentResponseModel<string>> CreditAsync(StoreSettleRequest req, CancellationToken ct = default);
    }
}
