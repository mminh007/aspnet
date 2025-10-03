using User.Common.Models.Requests;
using User.Common.Models.Responses;

namespace User.BLL.External.Interfaces
{
    public interface IStoreApiClient
    {
        Task<UserApiResponse<Guid?>> RegisterStoreAsync(RegisterStoreModel model);
    }
}
