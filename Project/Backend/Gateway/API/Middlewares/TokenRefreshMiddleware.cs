using API.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;

namespace API.Middlewares
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenRefreshMiddleware> _logger;

        public TokenRefreshMiddleware(RequestDelegate next, ILogger<TokenRefreshMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // restorage orginal response
            var originalResponse = context.Response.Body;
            using var newResponse = new MemoryStream();
            context.Response.Body = newResponse;

            await _next(context);

            if (context.Response.StatusCode == 401)
            {
                var refreshToken = context.Request.Cookies["refreshToken"];
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    using var httpClient = new HttpClient();
                    httpClient.BaseAddress = new Uri("https://localhost:7122");

                    var response = await httpClient.PostAsJsonAsync("/auth/refresh", new { });
                    if (response.IsSuccessStatusCode)
                    {
                        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseModel>();

                        // add new access token in cookie
                        context.Response.Cookies.Append("accessToken", tokenResponse!.AccessToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
                        });

                        // Retry request with new access token
                        context.Request.Headers["Authorization"] = "Bearer " + tokenResponse.AccessToken;

                        context.Response.Body = originalResponse;
                        context.Response.Clear();

                        await _next(context); // rerun pipeline
                        return;
                    }
                    else
                    {
                        context.Response.Body = originalResponse;
                        context.Response.Clear();
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Session expired. Please login again.");
                        return;
                    }
                }
            }

            // Copy body response
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await context.Response.Body.CopyToAsync(originalResponse);
        }
    }

    public static class TokenRefreshMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenRefresh(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenRefreshMiddleware>();
        }
    }
}
