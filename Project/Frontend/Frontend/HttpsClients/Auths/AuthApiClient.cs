using Frontend.Models.Auth;
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

        public async Task<(bool Success, string? AccessToken, string? RefreshToken, int ExpiresIn, string? Message, int statusCode, string Role)> LoginAsync(LoginModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("/auth/login", model);
            var content = await response.Content.ReadAsStringAsync();
            var statusCode = (int)response.StatusCode;

            var result = JsonSerializer.Deserialize<AuthApiResponse<TokenData>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!response.IsSuccessStatusCode || result?.Data == null)
            {
                return (false, null, null, 0, result?.Message ?? "Login Failed", statusCode, null);
            }

            return (true,
                result.Data.AccessToken,
                result.Data.RefreshToken,
                result.Data.ExpiresIn,
                result.Message,
                statusCode,
                result.Data.Roles);
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

            var result = JsonSerializer.Deserialize<AuthApiResponse<object>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!response.IsSuccessStatusCode || result == null)
            {
                return (false, result?.Message ?? $"Register Failed: {content}", statusCode);
            }

            return (true, result.Message, statusCode);
        }

        private class TokenData
        {
            [JsonPropertyName("accessToken")]
            public string AccessToken { get; set; }

            [JsonPropertyName("refreshToken")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("expiresIn")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("Roles")]
            public string Roles { get; set; }
        }

        private class AuthApiResponse<T>
        {
            [JsonPropertyName("statusCode")]
            public int StatusCode { get; set; }

            [JsonPropertyName("message")]
            public string? Message { get; set; }

            [JsonPropertyName("data")]
            public T? Data { get; set; }
        }

    }
}
