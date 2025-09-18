using Frontend.Cache.Interfaces;
using Frontend.HttpsClients.Orders;
using Frontend.Models.Orders;
using Frontend.Models.Orders.Requests;
using Frontend.Services.Interfaces;

namespace Frontend.Services
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly IOrderApiClient _client;

        private readonly IRedisCacheService _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public OrderService(ILogger<OrderService> logger, IOrderApiClient ApiClient, IRedisCacheService cache)
        {
            _logger = logger;
            _client = ApiClient;
            _cache = cache;
        }

        public Task<(string Message, int CountItems)> AddProductToCache(Guid userId, Guid cartId, Guid productId)
        {
            var listProductIds = new List<Guid>();
            string cacheKey = $"productList:userId:{userId}";

            var checkData = _cache.GetAsync<List<Guid>>(cacheKey);
            if (checkData != null)
            {
                checkData.Result.Add(productId);
                _cache.SetAsync(cacheKey, checkData.Result, _cacheDuration);

                var newCartItem = new RequestItemsToCartModel
                {
                    ProductId = productId,
                    Quantity = 1,

                },

            }

            listProductIds.Add(productId);
            _cache.SetAsync(cacheKey, listProductIds, _cacheDuration);

            // Get Counting Items
            var countItems = _cache.GetAsync<DTOs.CountItemsDTO>($"cart:countItems:{userId}");
            if (countItems != null) 
            {

            } 
        }

        public async Task<(string Message, int StatusCode, DTOs.CountItemsDTO? dto)> CountingItemsInCart(Guid userId)
        {
            string cacheKey = $"cart:countItems:{userId}";

            // Check cache first
            var checkData = await _cache.GetAsync<DTOs.CountItemsDTO?>(cacheKey);
            if (checkData != null)
            {
                _logger.LogInformation($"Redis Cache CountItems: {checkData.CountItems}");
                _logger.LogInformation($"Load Data from Redis for userId: {userId}");
                return ("OK (from cache)", 200, checkData);
            }

            // Call API if not in cache
            var (success, message, statusCode, dto) = await _client.GetCountItemsToCart(userId);
            if (!success)
            {
                return (message, statusCode, null);
            }

            // Store in cache
            await _cache.SetAsync(cacheKey, dto, _cacheDuration);
            return (message, statusCode, dto);

        }
    }
}
