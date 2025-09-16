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
        public async Task<(string message, int statusCode, IEnumerable<StoreDto?>)> GetAllStoresActiveAsync()
        {
            var (success, message, statusCode, data) = await _storeApiClient.GetAllStoresActiveAsync();

            if (!success)
            {
                return (message, statusCode, Enumerable.Empty<StoreDto?>());
            }

            return (message, statusCode, data);
        }

        public Task<(string message, int statusCode, StoreDto?)> GetStoresDetailAsync(Guid storeId)
        {
            throw new NotImplementedException();
        }
    }
}
