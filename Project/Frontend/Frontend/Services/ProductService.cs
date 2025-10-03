using Frontend.Cache.Interfaces;
using Frontend.HttpsClients.Products;
using Frontend.Models.Products;
using Frontend.Services.Interfaces;

namespace Frontend.Services
{
    public class ProductService : IProductService
    {
        private readonly ILogger _logger;
        private readonly IProductApiClient _productApiClient;

        private readonly IRedisCacheService _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public ProductService(IProductApiClient productApiClient,
                              ILogger<ProductService> logger,
                              IRedisCacheService cache)
        {
            _productApiClient = productApiClient;
            _logger = logger;
            _cache = cache;
        }


        public async Task<(string Message, int StatusCode, IEnumerable<DTOs.ProductBuyerDTO?>)> GetProductByStoreIdAsync(Guid storeId)
        {
            string cacheKey = $"products:store:{storeId}";

            // Check cache first
            //var cachedData = await _cache.GetAsync<IEnumerable<DTOs.ProductBuyerDTO>>(cacheKey);
            //if (cachedData != null)
            //{
            //    _logger.LogInformation("✅ Get Products from Redis for StoreId={StoreId}", storeId);
            //    return ("OK (from cache)", 200, cachedData);
            //}

            // Call API if not in cache
            var (success, message, statusCode, data) = await _productApiClient.GetByStoreAsync(storeId);
            if (!success || data == null)
            {
                return (message, statusCode, Enumerable.Empty<DTOs.ProductBuyerDTO?>());
            }

            // Store in cache
            await _cache.SetAsync(cacheKey, data, _cacheDuration);

            return (message, statusCode, data);
        }


        public async Task<(string Message, int StatusCode, IEnumerable<DTOs.CategoryDTO?>)> SearchCategoriesByStoreIdAsync(Guid storeId)
        {
            string cacheKey = $"categories:store:{storeId}";

            // 1️⃣ Check cache first
            var cachedData = await _cache.GetAsync<IEnumerable<DTOs.CategoryDTO>>(cacheKey);
            if (cachedData != null)
            {
                _logger.LogInformation("✅ Get Categories from Redis for StoreId={StoreId}", storeId);
                return ("OK (from cache)", 200, cachedData);
            }

            // call API if not in cache
            var (success, message, statusCode, data) = await _productApiClient.SearchCategoriesAsync(storeId);
            if (!success || data == null)
            {
                return (message, statusCode, Enumerable.Empty<DTOs.CategoryDTO?>());
            }

            // Store in cache
            await _cache.SetAsync(cacheKey, data, _cacheDuration);

            return (message, statusCode, data);
        }

        public async Task ClearProductsCache(Guid storeId)
        {
            string cacheKey = $"products:store:{storeId}";
            await _cache.RemoveAsync(cacheKey);

            string catKey = $"categories:store:{storeId}";
            await _cache.RemoveAsync(catKey);
        }


    }
}

//public async Task<(string Message, int StatusCode, DTOs.ProductBuyerDTO?)> GetProductByIdAsync(Guid productId)
//{
//    var (success, message, statusCode, data) = await _productApiClient.GetByIdAsync(productId);
//    if (!success)
//    {
//        return (message, statusCode, null);
//    }
//    return (message, statusCode, data);
//}