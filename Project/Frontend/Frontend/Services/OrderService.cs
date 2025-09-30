using Frontend.Cache.Interfaces;
using Frontend.HttpsClients.Orders;
using Frontend.Models.Orders;
using Frontend.Models.Orders.Requests;
using Frontend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Net.NetworkInformation;
// UserId không cần phải truyền vào vì backend sẽ parse từ access token
// Nhưng vì theo flow cũ nên vẫn sẽ truyền useId để tránh refactor quá nhiều và tránh bug
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

        public async Task<(string Message, int StatusCode, int CountItems, IEnumerable<DTOs.CartItemDTO> itemList)> AddProductToCart(Guid userId, RequestItemsToCartModel dto)
        {
            var (success, message, statusCode, cart) = await _client.AddItemsToCart(userId, dto);

            if (!success)
            {
                _logger.LogWarning("❌ AddItemsToCart fail for userId={UserId}, statusCode={StatusCode}, message={Message}",
                    userId, statusCode, message);

                // Giữ cache cũ nếu có
                var (msg, sttCode, dtoCount) = await CountingItemsInCart(userId);

                return ($"Message from API CountingItems: {msg}", sttCode, dtoCount.CountItems, null);
            }

            var totalItems = cart.Items.Count;

            // filter by storeId
            var productList = new List<DTOs.CartItemDTO>();

            foreach (var item in cart.Items)
            {
                if (item.StoreId == dto.StoreId) { productList.Add(item); }
            }

            // ✅ Luôn update cache bằng kết quả từ API
            string cacheKey_counting = $"cart:countItems:{userId}";
            await _cache.SetAsync(cacheKey_counting, new DTOs.CountItemsDTO { CountItems = totalItems }, _cacheDuration);
            _logger.LogInformation("✅ Cart updated for userId={UserId}, totalItems={TotalItems}", userId, totalItems);

            // Update Cart cache
            string cacheKey_cart = $"cart:Bagde:{userId}";
            await _cache.SetAsync(cacheKey_cart, cart, _cacheDuration);

            //var(_, _, _) = await GetCartByUserId(userId, "add");

            return (message, statusCode, totalItems, productList);
        }

        public async Task<(string Message, int StatusCode, IEnumerable<DTOs.CartItemDTO> itemList)> GetCartInStore(Guid userId, Guid storeId)
        {
            string cacheKey = $"cart:cartStore:{userId}-{storeId}";
            var (success, message, statusCode, data) = await _client.GetCartItemInStore(storeId);

            await _cache.SetAsync(cacheKey, data, _cacheDuration);

            return (message, statusCode, data);

        }


        public async Task<(string Message, int StatusCode, DTOs.CountItemsDTO data)> CountingItemsInCart(Guid userId)
        {
            string cacheKey = $"cart:cart:{userId}";

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


        public async Task<(string Message, int StatusCode, DTOs.OrderDTO Data)> GetOrderById(Guid orderId)
        {
            var result = await _client.GetOrderById(orderId);

            return (result.Message ?? "No message", result.statusCode, result.data);
        }

        public async Task<(string Message, int StatusCode, List<DTOs.OrderDTO> Data)> GetOrdersByUser(Guid userId)
        {
            var result = await _client.GetOrdersByUser(userId);
            return (result.Message ?? "No message", result.statusCode, result.data ?? new List<DTOs.OrderDTO>());
        }

 

        public async Task<(string Message, int StatusCode)> DeleteOrder(Guid orderId)
        {
            var result = await _client.DeleteOrder(orderId);
            return (result.Message ?? "No message", result.statusCode);
        }

        public async Task<(string Message, int StatusCode, IEnumerable<DTOs.OrderDTO> Data)> Checkout(Guid userId, IEnumerable<Guid> productIds)
        {
            var result = await _client.Checkout(userId, productIds);

            if (result.data != null)
            {
                // Sau khi checkout thành công, giỏ hàng sẽ thay đổi → gọi API GetCart để lấy giỏ hàng mới
                var (success, msg, status, newCart) = await _client.GetCart(userId);

                if (success)
                {
                    // ✅ Update lại cache countItems
                    string cacheKey_counting = $"cart:countItems:{userId}";
                    await _cache.SetAsync(cacheKey_counting, new DTOs.CountItemsDTO { CountItems = newCart.Items.Count }, _cacheDuration);

                    // ✅ Update lại cache cart
                    string cacheKey_cart = $"cart:Bagde:{userId}";
                    await _cache.SetAsync(cacheKey_cart, newCart, _cacheDuration);

                    _logger.LogInformation("🛒 Checkout done, cart refreshed for userId={UserId}, totalItems={TotalItems}", userId, newCart.TotalItems);
                }
                else
                {
                    _logger.LogWarning("⚠️ Failed to refresh cart after checkout for userId={UserId}", userId);
                }
            }

            return (result.Message ?? "No message", result.statusCode, result.data);
        }


        public async Task<(string Message, int StatusCode, DTOs.CartDTO data)> DeleteItemInCart(Guid userId, Guid cartItemId)
        {
            var result = await _client.RemoveItem(userId, cartItemId);

            if (result.data != null)
            {
                // Update lại cache
                string cacheKey_counting = $"cart:countItems:{userId}";
                await _cache.SetAsync(cacheKey_counting, new DTOs.CountItemsDTO { CountItems = result.data.Items.Count }, _cacheDuration);

                string cacheKey_cart = $"cart:Bagde:{userId}";
                await _cache.SetAsync(cacheKey_cart, result.data, _cacheDuration);

                _logger.LogInformation("🗑 Cart item deleted for userId={UserId}, totalItems={TotalItems}", userId, result.data.TotalItems);
            }

            return (result.Message ?? string.Empty, result.statusCode, result.data);
        }

    }
}
