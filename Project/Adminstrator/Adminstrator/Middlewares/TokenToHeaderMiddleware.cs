using Microsoft.AspNetCore.Http;

namespace Adminstrator.Middlewares
{
    public class TokenToHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenToHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var accessToken = context.Request.Cookies["accessToken"];
            if (!string.IsNullOrEmpty(accessToken) && !context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Request.Headers.Append("Authorization", $"Bearer {accessToken}");
            }

            await _next(context);
        }
    }

    public static class TokenToHeaderMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenToHeader(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenToHeaderMiddleware>();
        }
    }
}
