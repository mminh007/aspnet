
using Adminstrator.Services.Interfaces;

namespace Adminstrator.Middlewares
{
    public class RefreshTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public RefreshTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            var accessToken = context.Request.Cookies["admin_accessToken"];
            var refreshToken = context.Request.Cookies["admin_refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                // Nếu AccessToken hết hạn thì gọi refresh
                var parsed = Adminstrator.Helpers.AuthHelper.ParseUserIdFromToken(accessToken);
                if (!parsed.isAuth)
                {
                    var result = await authService.RefreshToken(refreshToken);
                    if (result.Success && !string.IsNullOrEmpty(result.newAccessToken))
                    {
                        context.Response.Cookies.Append("admin_accessToken", result.newAccessToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Expires = DateTime.UtcNow.AddSeconds(result.expiresIn)
                        });

                        if (!string.IsNullOrEmpty(result.newRefreshToken))
                        {
                            context.Response.Cookies.Append("admin_refreshToken", result.newRefreshToken, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.None,
                                Expires = DateTime.UtcNow.AddDays(7)
                            });
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
