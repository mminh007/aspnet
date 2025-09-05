using Frontend.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.HttpsClients.Auths
{
    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string? AccessToken, string? RefreshToken, int ExpiresIn, string? Message, int statusCode)> LoginAsync(LoginModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("/auth/login", model);
            var content = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorObj = JsonSerializer.Deserialize<AuthApiResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (false, null, null, 0, errorObj?.Message ?? "Login Failed", statusCode);
                }
                catch
                {
                    return (false, null, null, 0, $"Login Failed: {content}", statusCode);
                }
            }

            var result = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return (true,
                result?.AccessToken,
                result?.RefreshToken,
                result?.ExpiresIn ?? 1800,
                result?.Message,
                statusCode);
        }

        public async Task<(bool Success, string? Message, int statusCode)> RegisterAsync(RegisterModel model)
        {
            var payload = new
            {
                Email = model.EmailAddress,
                model.Password,
                model.Role
            };

            var response = await _httpClient.PostAsJsonAsync("/auth/register", payload);
            var content = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorObj = JsonSerializer.Deserialize<AuthApiResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (false, errorObj?.Message ?? "Register Failed", statusCode);
                }
                catch
                {
                    return (false, $"Register Failed: {content}", statusCode);
                }
            }

            var result = JsonSerializer.Deserialize<AuthApiResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return (true, result?.Message, statusCode);
        }

        private class LoginResponse
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("statusCode")]
            public int StatusCode { get; set; }
        }

        private class AuthApiResponse
        {
            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("statusCode")]
            public int StatusCode { get; set; }
        }
    }
}
