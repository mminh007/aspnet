using Microsoft.AspNetCore.Http;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frontend.Middlewares
{
    public class SessionDebugMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConnectionMultiplexer _redis;

        public SessionDebugMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
        {
            _next = next;
            _redis = redis;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Chỉ kiểm tra khi user gọi /debug/session
            if (context.Request.Path.StartsWithSegments("/debug/session"))
            {
                await context.Session.LoadAsync(); // Đảm bảo session đã load
                var sessionId = context.Session.Id;

                // Redis DB 1 (nơi bạn lưu session)
                var db = _redis.GetDatabase(1);
                var redisKey = $"Frontend_Session_{sessionId}";

                bool exists = await db.KeyExistsAsync(redisKey);
                var ttl = await db.KeyTimeToLiveAsync(redisKey);

                var response = new
                {
                    SessionId = sessionId,
                    ExistsInRedis = exists,
                    RedisKey = redisKey,
                    TimeToLiveSeconds = ttl?.TotalSeconds,
                    Message = exists
                        ? "✅ Session found in Redis"
                        : "❌ Session not found in Redis (check session configuration or Redis DB index)"
                };

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
                return;
            }

            await _next(context);
        }
    }
}
