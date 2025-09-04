using Backend.Authentication.Models;
using Backend.Authentication.Services;
using System.Net.Http.Headers;

namespace Backend.Authentication.HttpsClient
{
    public class UserApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserApiService> _logger;

        public UserApiService(HttpClient httpClient, ITokenService tokenService, ILogger<UserApiService> logger)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Gọi UserService để đăng ký user mới (sử dụng internal JWT với role=system)
        /// </summary>
        public async Task<HttpResponseMessage> RegisterUserAsync(CheckUserModel request)
        {
            try
            {
                // Tạo internal JWT cho service-to-service
                var internalToken = _tokenService.GenerateInternalServiceToken("AuthService");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", internalToken);

                _logger.LogInformation("Calling UserService /api/user/register for {Email}", request.Email);

                var response = await _httpClient.PostAsJsonAsync("/api/user/register", request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling UserService for {Email}", request.Email);
                throw;
            }
        }
    }
}
