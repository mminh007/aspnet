using Adminstrator.Configs.Auth;
using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Models.Auths.Requests;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Adminstrator.Models.Auths.DTOs;

namespace Adminstrator.HttpsClients.Auths
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
                          string? Message, int statusCode, string Role, bool? IsVerifyEmail)>
            LoginAsync(LoginModel model)
        {
            var response = await _httpClient.PostAsJsonAsync(_endpoints.Login, model);
            var result = await ParseResponse<TokenData>(response, "Login");

            return (result.Success, result.Data?.AccessToken, result.Data?.RefreshToken,
                    result.Data?.ExpiresIn ?? 0, result.Message, result.statusCode,
                    result.Data?.Roles ?? "", result.Data?.VerifyEmail);
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
            var result = await ParseResponse<TokenData>(response, "RefreshToken");

            return (result.Success, result.Data?.AccessToken, result.Data?.RefreshToken,
                    result.Data?.ExpiresIn ?? 0, result.Message, result.statusCode,
                    result.Data?.Roles ?? "");

        }


        public async Task<(bool Success, string? Message, int statusCode, string? Data)> VerifyEmailAsync(VerifyEmailRequest model)
        {
            var url = _endpoints.VerifyEmail;

            var response = await _httpClient.PostAsJsonAsync(url, model);

            return await ParseResponse<string>(response, "VerifyEmail");
        }

        public async Task<(bool Success, string? Message, int statusCode, string? Data)> ResendCodeAsync(ResendCodeRequest model)
        {
            var url = _endpoints.ResendCode;

            var response = await _httpClient.PostAsJsonAsync(url, model);

            return await ParseResponse<string>(response, "ResendCode");
        }

        public async Task<(bool Success, string? Message, int statusCode, string? Data)> ResetPasswordAsync(ResetPasswordRequestModel model)
        {
            var url = _endpoints.ResetPassword;

            var response = await _httpClient.PostAsJsonAsync(url, model);
            return await ParseResponse<string>(response, "ResetPassword");
        }

        public async Task<(bool Success, string? Message, int statusCode, string? Data)> ForgotPasswordAsync(string email)
        {
            var url = _endpoints.ForgotPassword.Replace("{email}", email);
            var response = await _httpClient.PostAsJsonAsync(url, email);

            return await ParseResponse<string>(response, "ForgotPassword");
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
                        (int)response.StatusCode, result.Data);
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
