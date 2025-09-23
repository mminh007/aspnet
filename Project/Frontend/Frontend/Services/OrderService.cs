using Frontend.Cache.Interfaces;
using Frontend.HttpsClients.Orders;
using Frontend.Models.Orders;
using Frontend.Models.Orders.Requests;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Net.NetworkInformation;

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

        public async Task<(string Message, int StatusCode, int CountItems)> AddProductToCart(Guid userId, RequestItemsToCartModel dto)
        {
            var (success, message, statusCode, totalItems) = await _client.AddItemsToCart(userId, dto);

            if (!success)
            {
                _logger.LogWarning("❌ AddItemsToCart fail for userId={UserId}, statusCode={StatusCode}, message={Message}",
                    userId, statusCode, message);

                // Giữ cache cũ nếu có
                var (msg, sttCode, dtoCount) = await CountingItemsInCart(userId);

                return ($"Message from API CountingItems: {msg}", sttCode, dtoCount.CountItems);
            }

            // ✅ Luôn update cache bằng kết quả từ API
            string cacheKey = $"cart:countItems:{userId}";
            await _cache.SetAsync(cacheKey, new DTOs.CountItemsDTO { CountItems = totalItems }, _cacheDuration);

            _logger.LogInformation("✅ Cart updated for userId={UserId}, totalItems={TotalItems}", userId, totalItems);

            var(_, _, _) = await GetCartByUserId(userId, "add");

            return (message, statusCode, totalItems);
        }


        public async Task<(string Message, int StatusCode, DTOs.CountItemsDTO data)> CountingItemsInCart(Guid userId)
        {
            string cacheKey = $"cart:countItems:{userId}";

            // ✅ Check cache first
            var checkData = await _cache.GetAsync<DTOs.CountItemsDTO>(cacheKey);
            if (checkData != null)
            {
                _logger.LogInformation("📦 Redis Cache Hit - userId={UserId}, countItems={CountItems}", userId, checkData.CountItems);
                return ("OK (from cache)", 200, checkData);
            }

            // ❌ Not in cache → Call API
            var (success, message, statusCode, dto) = await _client.GetCountItemsToCart(userId);

            if (!success || dto == null)
            {
                _logger.LogWarning("⚠️ CountingItems API fail for userId={UserId}, statusCode={StatusCode}, message={Message}",
                    userId, statusCode, message);

                // Trả về CountItems=0 thay vì null để tránh NullReference
                return (message, statusCode, new DTOs.CountItemsDTO { CountItems = 0 });
            }

            // ✅ Store in cache
            await _cache.SetAsync(cacheKey, dto, _cacheDuration);
            _logger.LogInformation("✅ Cache refreshed for userId={UserId}, countItems={CountItems}", userId, dto.CountItems);

            return (message, statusCode, dto);
        }

        public async Task<(string Message, int StatusCode, DTOs.CartDTO? data)> GetCartByUserId(Guid userId, string status = "add")
        {
            string cacheKey = $"cart:Bagde:{userId}";
            if (status == null)
            {

                var checkData = await _cache.GetAsync<DTOs.CartDTO>(cacheKey);

                if (checkData != null)
                {
                    return ("From Cache (200)", 200, checkData);
                }

                var (success, message, statusCode, data) = await _client.GetCart(userId);

                if (!success)
                {
                    return (message, statusCode, data);
                }

                await _cache.SetAsync(cacheKey, data, _cacheDuration);
                return (message, statusCode, data);
            }

            else
            {
                var (success, message, statusCode, data) = await _client.GetCart(userId);

                if (!success)
                {
                    return (message, statusCode, data);
                }

                await _cache.SetAsync(cacheKey, data, _cacheDuration);
                return (message, statusCode, data);
            }
     
        }

        public async Task<(string Message, int StatusCode, DTOs.CartDTO data)> UpdateItemsInCart(Guid userId, Guid cartItemId, UpdateQuantityModel request)
        {
            var result = await _client.UpdateItemQuantity(userId, cartItemId, request);

            return (result.Message, result.statusCode, result.data);
        }
    }
}
