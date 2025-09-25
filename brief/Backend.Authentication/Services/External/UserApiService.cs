using Backend.Authentication.Models.Requests;
using Backend.Authentication.Models.Responses;
using Backend.Authentication.Services;
using System.Net.Http.Headers;

namespace Backend.Authentication.Services.External
{
    public class UserApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenManager _tokenService;
        private readonly ILogger<UserApiService> _logger;

        public UserApiService(HttpClient httpClient, ITokenManager tokenService, ILogger<UserApiService> logger)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Gọi UserService để đăng ký user mới (sử dụng internal JWT với role=system)
        /// </summary>
        public async Task<RegisterUserResponseModel> RegisterUserAsync(CheckUserModel request)
        {
            try
            {
                // Tạo internal JWT cho service-to-service
                var internalToken = _tokenService.GenerateInternalServiceToken("AuthService");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", internalToken);

                _logger.LogInformation("Calling UserService /api/user/register for {Email}", request.Email);

                var response = await _httpClient.PostAsJsonAsync("/api/user/register", request);
                response.EnsureSuccessStatusCode();

                var userApiResponse = await response.Content.ReadFromJsonAsync<RegisterUserResponseModel>();

                if (userApiResponse == null || userApiResponse.UserId == Guid.Empty)
                {
                    _logger.LogWarning("UserService register response is null or invalid for {Email}", request.Email);
                    return null;
                }

                return new RegisterUserResponseModel
                {
                    StatusCode = userApiResponse.StatusCode,
                    UserId = userApiResponse.UserId,
                    Message = userApiResponse.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling UserService for {Email}", request.Email);
                throw;
            }
        }
    }
}
