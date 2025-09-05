using Frontend.Models.Users;

namespace Frontend.HttpsClients.Users
{
    public class UserApiClient : IUserApiClient
    {
        public Task<ProfileResponse> GetUserByIdAsync()
        {
            throw new NotImplementedException();
        }
    }
}
