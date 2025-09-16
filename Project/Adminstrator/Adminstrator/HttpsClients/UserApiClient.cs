using Adminstrator.HttpsClients.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Adminstrator.HttpsClients
{
    public class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private readonly string _getStoreByUserIdEndpoint;

        public UserApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            var endpoints = _configuration.GetSection("ServiceUrls:User:Endpoints");
            _getStoreByUserIdEndpoint = endpoints["GetStoreByUserId"];
        }

        public async Task<(bool Success, string? Message, int statusCode, List<Guid>)> GetStoreByUserIdAsync(Guid userId)
        {
            var url = _getStoreByUserIdEndpoint.Replace("{id}", userId.ToString());
            var response = await _httpClient.GetAsync(url);

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var result = JsonSerializer.Deserialize<UserApiResponse<List<Guid>>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result == null)
                {
                    return (false, result?.Message ?? $"Register Failed: {content}", (int)response.StatusCode, null);
                }

                return (true, result.Message, (int)response.StatusCode, result.Data);
            }
            catch (Exception ex)
            {
                return (false, $"Exception while parsing response: {ex.Message}", (int)response.StatusCode, null);
            }
        }

       

        // ✅ Private class cho user
        private class UserDto
        {
            [JsonPropertyName("userId")]
            public Guid UserId { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("storeId")]
            public Guid? StoreId { get; set; }

            [JsonPropertyName("address")]
            public string? Address { get; set; }

            [JsonPropertyName("phoneNumber")]
            public string? PhoneNumber { get; set; }

            [JsonPropertyName("createdAt")]
            public DateTime CreatedAt { get; set; }
        }

        // ✅ Private generic response wrapper
        private class UserApiResponse<T>
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
