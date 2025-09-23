using Frontend.Models.Auth;
using static Frontend.Models.Auth.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.HttpsClients.Auths
{
    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthApiClient> _logger;

        private readonly string _login;
        private readonly string _register;
        private readonly string _refresh;

        public AuthApiClient(HttpClient httpClient, ILogger<AuthApiClient> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = config;

            var endpoints = _configuration.GetSection("ServiceUrls:Auth:Endpoints");
            _login = endpoints["Login"];
            _register = endpoints["Register"];
            _refresh = endpoints["Refresh"];
        }

        public async Task<(bool Success, string? AccessToken, string? RefreshToken, int ExpiresIn, string? Message, int statusCode, string Role)>
            LoginAsync(LoginModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(_login, model);
            return await ParseResponse<TokenData>(response, "Login");
        }

        public async Task<(bool Success, string? Message, int statusCode)> RegisterAsync(RegisterModel model)
        {
            var payload = new { Email = model.EmailAddress, model.Password, model.Role };
            var response = await _httpClient.PostAsJsonAsync(_register, payload);
            var parsed = await ParseResponse<object>(response, "Register");
            return (parsed.Success, parsed.Message, parsed.statusCode);
        }

        public async Task<(bool Success, string? AccessToken, string? RefreshToken, int ExpiresIn, string? Message, int statusCode, string Role)>
            RefreshTokenAsync(string refreshToken)
        {
            // gửi refreshToken trong cookie
            var request = new HttpRequestMessage(HttpMethod.Post, _refresh);
            request.Headers.Add("Cookie", $"refreshToken={refreshToken}");

            var response = await _httpClient.SendAsync(request);
            return await ParseResponse<TokenData>(response, "RefreshToken");
        }

        private async Task<(bool Success, string? AccessToken, string? RefreshToken, int ExpiresIn, string? Message, int statusCode, string Role)>
            ParseResponse<T>(HttpResponseMessage response, string action)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<AuthApiResponse<TokenData>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result?.Data == null)
                {
                    return (false, null, null, 0, result?.Message ?? $"[{action}] Failed: {content}", (int)response.StatusCode, null);
                }

                return (true,
                    result.Data.AccessToken,
                    result.Data.RefreshToken,
                    result.Data.ExpiresIn,
                    result.Message,
                    result.StatusCode,
                    result.Data.Roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Exception while parsing response for {action}");
                return (false, null, null, 0, $"Exception: {ex.Message}", (int)response.StatusCode, null);
            }
        }

      

        private class AuthApiResponse<T>
        {
            [JsonPropertyName("statusCode")] public int StatusCode { get; set; }
            [JsonPropertyName("message")] public string? Message { get; set; }
            [JsonPropertyName("data")] public T? Data { get; set; }
        }
    }
}
