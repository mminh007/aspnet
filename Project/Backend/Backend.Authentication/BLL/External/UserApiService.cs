using BLL.Services.Interfaces;
using Commons.Enums;
using Commons.Models.Requests;
using Commons.Models.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace BLL.External
{
    public class UserApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenManager _tokenService;
        private readonly ILogger<UserApiService> _logger;
        private readonly string _registerEndpoint;

        public UserApiService(HttpClient httpClient, ITokenManager tokenService, ILogger<UserApiService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _logger = logger;

            var baseUrl = configuration["ServiceUrls:User:BaseUrl"];
            var registerPath = configuration["ServiceUrls:User:Endpoints:Register"];

            if(string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(registerPath))
                throw new ArgumentException("⚠️ User service URL is not configured properly in appsettings.json");

            _httpClient.BaseAddress = new Uri(baseUrl);
            _registerEndpoint = registerPath;

        }

    
        public async Task<AuthResponseModel<Guid>> RegisterUserAsync(CheckUserModel request)
        {
            try
            {
                // Tạo internal JWT cho service-to-service
                var internalToken = _tokenService.GenerateInternalServiceToken("AuthService");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", internalToken);

                //_logger.LogInformation("👉 Outgoing Authorization Header: {Header}",
                //    _httpClient.DefaultRequestHeaders.Authorization?.ToString());

                _logger.LogInformation("Calling UserService {Endpoint} for {Email}", _registerEndpoint, request.Email);

                var response = await _httpClient.PostAsJsonAsync(_registerEndpoint, request);

                var raw = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Raw response from UserService ({StatusCode}): {Raw}",
                    (int)response.StatusCode, raw);


                var userApiResponse = System.Text.Json.JsonSerializer.Deserialize<AuthResponseModel<Guid>>(raw,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return userApiResponse ?? new AuthResponseModel<Guid>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Failed to parse UserService response",
                    Data = Guid.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling UserService for {Email}", request.Email);
                return new AuthResponseModel<Guid>
                {
                    Message = OperationResult.Error,
                    ErrorMessage = "Unexpected error while calling UserService",
                    Data = Guid.Empty
                };
            }
        }
    }
}
