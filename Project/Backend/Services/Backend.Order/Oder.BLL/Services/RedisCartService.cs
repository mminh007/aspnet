using StackExchange.Redis;
using System.Text.Json;
using static Order.Common.Models.DTOs;

namespace Order.BLL.Services
{
    public class RedisCartService
    {
        private readonly IDatabase _redisDb;
        private readonly string _cartPrefix = "cart:";

        public RedisCartService(IConnectionMultiplexer redis)
        {
            _redisDb = redis.GetDatabase();
        }

        private string GetCartKey(Guid userId) => $"{_cartPrefix}{userId}";

        public async Task<CartDTO?> GetCartAsync(Guid userId)
        {
            var cartJson = await _redisDb.StringGetAsync(GetCartKey(userId));
            return cartJson.IsNullOrEmpty ? null : JsonSerializer.Deserialize<CartDTO>(cartJson!);
        }

        public async Task SaveCartAsync(Guid userId, CartDTO cart, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(cart);
            await _redisDb.StringSetAsync(GetCartKey(userId), json, expiry ?? TimeSpan.FromDays(7));
        }

        public async Task RemoveCartAsync(Guid userId)
        {
            await _redisDb.KeyDeleteAsync(GetCartKey(userId));
        }
    }
}
