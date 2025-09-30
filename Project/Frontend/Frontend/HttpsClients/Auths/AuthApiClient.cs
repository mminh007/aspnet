
using Frontend.Configs.Auth;
using Frontend.Models.Auth;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.HttpsClients.Auths
{
    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthApiClient> _logger;
        private readonly AuthEndpoints _endpoints;

        public AuthApiClient(HttpClient httpClient,
                             ILogger<AuthApiClient> logger,
                             IOptions<AuthEndpoints> endpoints)
        {
            _httpClient = httpClient;
            _logger = logger;
            _endpoints = endpoints.Value;
        }

        public async Task<(bool Success, string? AccessToken, string? RefreshToken, int ExpiresIn,
                          string? Message, int statusCode, string Role)>
            LoginAsync(LoginModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(_endpoints.Login, model);
            return await ParseResponse(response, "Login");
        }

        public async Task<(bool Success, string? Message, int statusCode)> RegisterAsync(RegisterModel model)
        {
            var payload = new { Email = model.EmailAddress, model.Password, model.Role };
            var response = await _httpClient.PostAsJsonAsync(_endpoints.Register, payload);
            var parsed = await ParseResponse<object>(response, "Register");
            return (parsed.Success, parsed.Message, parsed.statusCode);
        }

        public async Task<(bool Success, string? AccessToken, string? RefreshToken, int ExpiresIn,
                          string? Message, int statusCode, string Role)>
            RefreshTokenAsync(string refreshToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _endpoints.Refresh);
            request.Headers.Add("Cookie", $"refreshToken={refreshToken}");

            var response = await _httpClient.SendAsync(request);
            return await ParseResponse(response, "RefreshToken");
        }

        private async Task<(bool Success, string? AccessToken, string? RefreshToken, int ExpiresIn,
                           string? Message, int statusCode, string Role)>
            ParseResponse(HttpResponseMessage response, string action)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<AuthApiResponse<DTOs.TokenData>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result?.Data == null)
                {
                    return (false, null, null, 0,
                        result?.Message ?? $"[{action}] Failed: {content}",
                        (int)response.StatusCode, null);
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
                _logger.LogError(ex, "❌ Exception while parsing {Action}", action);
                return (false, null, null, 0,
                    $"Exception: {ex.Message}", (int)response.StatusCode, null);
            }
        }

        private async Task<(bool Success, string? Message, int statusCode, T? Data)>
            ParseResponse<T>(HttpResponseMessage response, string action)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<AuthApiResponse<T>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result == null)
                {
                    return (false, result?.Message ?? $"[{action}] Failed: {content}",
                        (int)response.StatusCode, default);
                }

                return (true, result.Message, result.StatusCode, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while parsing {Action}", action);
                return (false, $"Exception: {ex.Message}", (int)response.StatusCode, default);
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
