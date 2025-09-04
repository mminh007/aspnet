using Backend.Authentication.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace Backend.Authentication.HttpsClient
{
    public class UserApiService
    {
        private readonly HttpClient _httpClient;

        public UserApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> RegisterUserAsync(CheckUserModel request)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/user/register", request);

            return response;
        }
    }
}
