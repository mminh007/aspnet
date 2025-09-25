using Backend.User.Models.Requests;
using Backend.User.Models.Responses;

namespace Backend.User.Services.External
{
    public interface IStoreApiClient
    {
        Task<UserApiResponse<object>> RegisterStoreAsync(RegisterStoreModel model);
    }
}
