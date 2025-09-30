using Adminstrator.Helpers;
using Adminstrator.Services.Interfaces;
using System.Reflection;

namespace Adminstrator.Middlewares
{
    public class SessionRestoreMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionRestoreMiddleware> _logger;

        public SessionRestoreMiddleware(RequestDelegate next, ILogger<SessionRestoreMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            var userEmail = context.Session.GetString("UserEmail");
            var accessToken = context.Request.Cookies["admin_accessToken"];
            var refreshToken = context.Request.Cookies["admin_refreshToken"];

            // Nếu Session mất nhưng cookie còn
            if (string.IsNullOrEmpty(userEmail) && !string.IsNullOrEmpty(accessToken))
            {
                var (isAuth, userId, Email, Role) = AuthHelper.ParseUserIdFromToken(accessToken);

                if (!isAuth && !string.IsNullOrEmpty(refreshToken))
                {
                    // Token hết hạn => gọi Refresh API
                    var (success, newAccessToken, newRefresToken, expiresIn, role, message, statusCode)
                        = await authService.RefreshToken(refreshToken);

                    if (success && !string.IsNullOrEmpty(newAccessToken))
                    {
                        context.Response.Cookies.Append("admin_accessToken", newAccessToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Expires = DateTime.UtcNow.AddSeconds(expiresIn)
                        });

                        context.Session.SetString("UserEmail", Email);
                        context.Session.SetString("UserRole", role);
                        _logger.LogInformation("✅ Session restored from refresh token");
                    }
                }
                else if (isAuth)
                {
                    // Token còn hạn => restore session
                    context.Session.SetString("UserRole", Role);
                    context.Session.SetString("UserEmail", Email);
                    context.Session.SetString("UserId", userId);

                    _logger.LogInformation("✅ Session restored from access token");
                }
            }

            await _next(context);
        }
    }

}
