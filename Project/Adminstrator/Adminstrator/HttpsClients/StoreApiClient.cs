using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Models.Stores;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Adminstrator.HttpsClients
{
    public class StoreApiClient : IStoreApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StoreApiClient> _logger; //= LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<StoreApiClient>();

        private readonly string _getByUserIdEndpoint;
        private readonly string _getByIdEndpoint;

        public StoreApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<StoreApiClient> logger )
        {
            _httpClient = httpClient;
            _configuration = configuration;


            var endpoints = _configuration.GetSection("ServiceUrls:Store:Endpoints");
            _getByUserIdEndpoint = endpoints["GetByUserId"];
            _getByIdEndpoint = endpoints["GetById"];

            _logger = logger;
        }

        public async Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> GetByUserIdAsync(Guid userId)
        {
            var url = _getByUserIdEndpoint.Replace("{userId}", userId.ToString());

            // 🔍 Debug: Log request details
            _logger.LogInformation("🔍 Making request to: {Url}", url);

            // Log Authorization header
            var authHeader = _httpClient.DefaultRequestHeaders.Authorization;
            if (authHeader != null)
            {
                _logger.LogInformation("🔍 HttpClient Authorization header: {Scheme} {Token}",
                    authHeader.Scheme, authHeader.Parameter?[..10] + "...");
            }
            else
            {
                _logger.LogWarning("⚠️ No Authorization header set on HttpClient");
            }

            // Log all headers
            _logger.LogInformation("🔍 All HttpClient headers:");
            foreach (var header in _httpClient.DefaultRequestHeaders)
            {
                _logger.LogInformation("  {Key}: {Value}", header.Key, string.Join(", ", header.Value));
            }

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            // 🔍 Debug: Log response
            _logger.LogInformation("🔍 Response Status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("🔍 Response Content Length: {Length}", content?.Length ?? 0);

            try
            {
                var result = JsonSerializer.Deserialize<StoreApiResponse<StoreDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result == null)
                {
                    _logger.LogWarning("⚠️ Request failed. Status: {Status}, Content: {Content}",
                        response.StatusCode, content?[..200]);
                    return (false, result?.Message ?? $"Request failed: {content}", (int)response.StatusCode, null);
                }

                _logger.LogInformation("✅ Request successful. Message: {Message}", result.Message);
                return (true, result.Message, result.StatusCode, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while parsing response: {Message}", ex.Message);
                return (false, $"Exception while parsing response: {ex.Message}", (int)response.StatusCode, null);
            }
        }


        public async Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> GetByIdAsync(Guid storeId)
        {
            var url = _getByIdEndpoint.Replace("{storeId}", storeId.ToString());

            _logger.LogInformation("🔍 Making request to: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("🔍 Response Status: {StatusCode}", response.StatusCode);

            try
            {
                var result = JsonSerializer.Deserialize<StoreApiResponse<StoreDto>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!response.IsSuccessStatusCode || result == null)
                {
                    return (false, result?.Message ?? $"Request failed: {content}", (int)response.StatusCode, null);
                }

                return (true, result.Message, result.StatusCode, result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while parsing response: {Message}", ex.Message);
                return (false, $"Exception while parsing response: {ex.Message}", (int)response.StatusCode, null);
            }
        }


        // ✅ Private generic response wrapper
        private class StoreApiResponse<T>
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
