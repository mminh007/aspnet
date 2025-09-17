using Frontend.HttpsClients.Stores;
using Frontend.Models.Stores;
using Frontend.Services.Interfaces;

namespace Frontend.Services
{
    public class StoreService : IStoreService
    {
        private readonly IStoreApiClient _storeApiClient;
        public StoreService(IStoreApiClient storeApiClient)
        {
            _storeApiClient = storeApiClient;
        }
        public async Task<(string message, int statusCode, IEnumerable<StoreDto?>)> GetStoresPagedAsync(int page, int pageSize)
        {
            var (success, message, statusCode, data) = await _storeApiClient.GetStoresPagedAsync(page, pageSize);

            if (!success)
            {
                return (message, statusCode, Enumerable.Empty<StoreDto?>());
            }

            return (message, statusCode, data);
        }

        public async Task<(string message, int statusCode, StoreDto?)> GetStoresDetailAsync(Guid storeId)
        {
            var (success, message, statusCode, data) = await _storeApiClient.GetStoreByIdAsync(storeId);
            if (!success)
            {
                return (message, statusCode, (StoreDto?)null);
            }
            return (message, statusCode, data);
        }
    }
}
