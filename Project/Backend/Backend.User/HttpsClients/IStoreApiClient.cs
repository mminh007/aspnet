using Backend.User.Models;

namespace Backend.User.HttpsClients
{
    public interface IStoreApiClient
    {
        Task<UserApiResponse<object>> RegisterStoreAsync(RegisterStoreModel model);
    }
}
