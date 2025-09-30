using Adminstrator.Configs.Store;
using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Models.Stores;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Adminstrator.HttpsClients
{
    public class StoreApiClient : IStoreApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StoreApiClient> _logger;
        private readonly StoreEndpoints _endpoints;

        public StoreApiClient(HttpClient httpClient,
                              ILogger<StoreApiClient> logger,
                              IOptions<StoreEndpoints> endpoints)
        {
            _httpClient = httpClient;
            _logger = logger;
            _endpoints = endpoints.Value;
        }

        public async Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> GetByUserIdAsync()
        {
            var url = _endpoints.GetByUserId;
            var response = await _httpClient.GetAsync(url);

            return await ParseResponse<StoreDto>(response, "GetStoreBuUser");
        }


        public async Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> GetByIdAsync(Guid storeId)
        {
            var url = _endpoints.GetById.Replace("{storeId}", storeId.ToString());
            var response = await _httpClient.GetAsync(url);

            return await ParseResponse<StoreDto>(response, "GetStoreById");
        }

        public async Task<(bool Success, string? Message, int statusCode, StoreDto? Data)> UpdateInfomationStore(UpdateStoreModel model)
        {
            var url = _endpoints.Update.Replace("{storeId}", model.StoreId.ToString());
            var response = await _httpClient.PutAsJsonAsync(url, model);

            return await ParseResponse<StoreDto>(response, "UpdateStore");
        }


        public async Task<(bool Success, string? Message, int statusCode)> ChangeActiveStore(ChangeActiveRequest request)
        {
            var url = _endpoints.ChangeActive.Replace("{storeId}", request.StoreId.ToString());
            var content = JsonContent.Create(request);

            var patchRequest = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(patchRequest);

            var results = await ParseResponse<object>(response, "ChangeActiveStore");

            return (results.Success, results.Message, results.statusCode);
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
