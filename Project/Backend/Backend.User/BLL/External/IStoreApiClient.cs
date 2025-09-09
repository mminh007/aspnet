using Commons.Models.Requests;
using Commons.Models.Responses;

namespace BLL.External
{
    public interface IStoreApiClient
    {
        Task<UserApiResponse<object>> RegisterStoreAsync(RegisterStoreModel model);
    }
}
