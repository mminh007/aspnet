using Frontend.Models.Users;

namespace Frontend.HttpsClients.Users
{
    public interface IUserApiClient
    {
        Task<ProfileResponse> GetUserByIdAsync();
    }
}
