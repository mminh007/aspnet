using Frontend.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Frontend.HttpsClient
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string? Token, string? Message)> LoginAsync(LoginModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("/auth/login", model);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorObj = JsonSerializer.Deserialize<ApiResponse>(content);
                    return (false, null, errorObj?.Message ?? "Đăng nhập thất bại");
                }
                catch
                {
                    return (false, null, $"Đăng nhập thất bại: {content}");
                }
            }

            var result = JsonSerializer.Deserialize<LoginResponse>(content);
            return (true, result?.AccessToken, result?.Message);
        }

        public async Task<(bool Success, string? Message)> RegisterAsync(RegisterModel model)
        {
            var payload = new
            {
                Email = model.EmailAddress,
                Password = model.Password,
                Role = model.Role
            };

            var response = await _httpClient.PostAsJsonAsync("/auth/register", payload);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorObj = JsonSerializer.Deserialize<ApiResponse>(content);
                    return (false, errorObj?.Message ?? "Đăng ký thất bại");
                }
                catch
                {
                    return (false, $"Đăng ký thất bại: {content}");
                }
            }

            var result = JsonSerializer.Deserialize<ApiResponse>(content);
            return (true, result?.Message);
        }

        private class LoginResponse
        {
            public string Message { get; set; }
            public string AccessToken { get; set; }
        }

        private class ApiResponse
        {
            public string Message { get; set; }
        }
    }
}
