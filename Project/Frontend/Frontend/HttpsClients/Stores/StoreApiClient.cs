using Frontend.Configs;
using Frontend.Configs.Store;
using Frontend.Helpers;
using Frontend.Models.Stores;
using Microsoft.Extensions.Options;
using Slugify;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frontend.HttpsClients.Stores
{
    public class StoreApiClient : IStoreApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StoreApiClient> _logger;
        private readonly StoreEndpoints _endpoints;
        private readonly ISlugHelper _slugHelper;

        public StoreApiClient(HttpClient httpClient,
                              ILogger<StoreApiClient> logger,
                              IOptions<StoreEndpoints> endpoints)
        {
            _httpClient = httpClient;
            _logger = logger;
            _endpoints = endpoints.Value;

            _slugHelper = new VietnameseSlugHelper();
           
        }

        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<StoreDto?>? Data)>
            GetStoresPagedAsync(int page, int pageSize)
        {
            var url = _endpoints.GetAllStore
                .Replace("{page}", page.ToString())
                .Replace("{pageSize}", pageSize.ToString());

            var response = await _httpClient.GetAsync(url);
            return await ParseResponse<IEnumerable<StoreDto>>(response, "GetStoresPaged");
        }

        public async Task<(bool Success, string? Message, int statusCode, StoreDto? Data)>
            GetStoreByIdAsync(Guid storeId)
        {
            var url = _endpoints.GetStoreDetail.Replace("{storeId}", storeId.ToString());
            var response = await _httpClient.GetAsync(url);
            return await ParseResponse<StoreDto>(response, "GetStoreById");
        }

        public async Task<(bool Success, string? Message, int statusCode, IEnumerable<StoreDto> data)> SearchStoreByKeywordAsync(string keyword)
        {
            var url = _endpoints.GetStoreByKeyword.Replace("{keyword}", keyword.ToString());
            var response = await _httpClient.GetAsync(url);

            return await ParseResponse<IEnumerable<StoreDto>>(response, "SearchByKeyword");
        }

        public async Task<(bool Success, string? Message, int statusCode, PaginatedStoreResponse data)>
            SearchStoreByTag(string tag, int page, int pageSize)
        {
            // ✅ Nếu tag null hoặc rỗng thì để trống
            string slugTag = string.Empty;

            if (!string.IsNullOrWhiteSpace(tag))
            {

                // "Đồ ăn" → "do-an", "Mỹ phẩm, Dược phẩm" → "my-pham-duoc-pham"
                slugTag = _slugHelper.GenerateSlug(tag);
            }

            var url = _endpoints.GetStoreByTag
                                .Replace("{tag}", slugTag)
                                .Replace("{page}", page.ToString())
                                .Replace("{pageSize}", pageSize.ToString());

            _logger.LogInformation("🔗 Requesting Store Search: {Url}", url);

            var response = await _httpClient.GetAsync(url);

            return await ParseResponse<PaginatedStoreResponse>(response, "SearchByTag");
        }


        private async Task<(bool Success, string? Message, int statusCode, T? Data)>
            ParseResponse<T>(HttpResponseMessage response, string action)
        {
            var content = await response.Content.ReadAsStringAsync();
            try
            {
                var result = JsonSerializer.Deserialize<StoreApiResponse<T>>(content,
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

     

        private class StoreApiResponse<T>
        {
            [JsonPropertyName("statusCode")] public int StatusCode { get; set; }
            [JsonPropertyName("message")] public string? Message { get; set; }
            [JsonPropertyName("data")] public T? Data { get; set; }
        }
    }
}
