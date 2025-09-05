using Frontend.Models.Stores;
using System.Text.Json;

namespace Frontend.HttpsClients.Stores
{
    public class StoreApiClient : IStoreApiClient
    {
        private readonly HttpClient _client;

        public StoreApiClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<StoreResponseModel>> GetStoresAsync()
        {
            var response = await _client.GetAsync("/stores/all");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error calling API: {content}");
            }

            var stores = JsonSerializer.Deserialize<List<StoreResponseModel>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return stores ?? new List<StoreResponseModel>();
        }
    }
}
