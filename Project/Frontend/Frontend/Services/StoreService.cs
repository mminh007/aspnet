using Frontend.Cache.Interfaces;
using Frontend.HttpsClients.Stores;
using Frontend.Models.Stores;
using Frontend.Services.Interfaces;

namespace Frontend.Services
{
    public class StoreService : IStoreService
    {
        private readonly IStoreApiClient _storeApiClient;
        private readonly IRedisCacheService _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
        public StoreService(IStoreApiClient storeApiClient, IRedisCacheService cache)
        {
            _storeApiClient = storeApiClient;
            _cache = cache;
        }

        public async Task<(string message, int statusCode, IEnumerable<StoreDto?>)> GetStoresPagedAsync(int page, int pageSize)
        {
            string cacheKey = $"stores:page:{page}:size:{pageSize}";

            // Check Cache first
            var cachedData = await _cache.GetAsync<IEnumerable<StoreDto>>(cacheKey);
            if (cachedData != null)
            {
                return ("OK (from cache)", 200, cachedData);
            }

            // Call API if not in cache
            var (success, message, statusCode, data) = await _storeApiClient.GetStoresPagedAsync(page, pageSize);

            if (!success || data == null)
            {
                return (message, statusCode, Enumerable.Empty<StoreDto?>());
            }

            // Store in Cache
            await _cache.SetAsync(cacheKey, data, _cacheDuration);

            return (message, statusCode, data);
        }

        public async Task<(string message, int statusCode, StoreDto?)> GetStoresDetailAsync(Guid storeId)
        {
            string cacheKey = $"store:{storeId}";

            var cachedData = await _cache.GetAsync<StoreDto>(cacheKey);
            if (cachedData != null)
            {
                return ("OK (from cache)", 200, cachedData);
            }

            var (success, message, statusCode, data) = await _storeApiClient.GetStoreByIdAsync(storeId);
            if (!success || data == null)
            {
                return (message, statusCode, (StoreDto?)null);
            }

            await _cache.SetAsync(cacheKey, data, _cacheDuration);
            return (message, statusCode, data);
        }
    }
}
