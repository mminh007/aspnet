using Order.BLL.External.Interfaces;

namespace Order.Helpers
{
    public class HeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HeaderHandler> _logger;
        private readonly IAuthApiClient _authApiClient;

        public HeaderHandler(IHttpContextAccessor httpContextAccessor, ILogger<HeaderHandler> logger, IAuthApiClient authApiClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _authApiClient = authApiClient;
            _logger = logger;
        }

        // Thêm vào HeaderHandler để debug chi tiết hơn
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            string? token = null;

            if (httpContext != null)
            {
                //_logger.LogInformation("🔍 === HEADER HANDLER DEBUG ===");
                //_logger.LogInformation("🔍 Request URL: {Url}", request.RequestUri);
                //_logger.LogInformation("🔍 Current User: {User}", httpContext.User?.Identity?.Name ?? "Anonymous");
                //_logger.LogInformation("🔍 Is Authenticated: {IsAuth}", httpContext.User?.Identity?.IsAuthenticated ?? false);

                // Log tất cả cookies
                //_logger.LogInformation("🔍 === COOKIES ===");
                //foreach (var cookie in httpContext.Request.Cookies)
                //{
                //    var value = cookie.Key == "accessToken" || cookie.Key == "refreshToken"
                //        ? $"{cookie.Value[..10]}..."
                //        : cookie.Value;
                //    _logger.LogInformation("  {Key}: {Value}", cookie.Key, value);
                //}

                // Log tất cả request headers
                //_logger.LogInformation("🔍 === REQUEST HEADERS ===");
                //foreach (var header in httpContext.Request.Headers)
                //{
                //    var value = header.Key == "Authorization"
                //        ? $"{string.Join(", ", header.Value)}".Substring(0, Math.Min(30, string.Join(", ", header.Value).Length)) + "..."
                //        : string.Join(", ", header.Value);
                //    _logger.LogInformation("  {Key}: {Value}", header.Key, value);
                //}

                // Log HttpClient headers trước khi xử lý
                //_logger.LogInformation("🔍 === HTTPCLIENT HEADERS (BEFORE) ===");
                //var beforeAuth = request.Headers.Authorization;
                //_logger.LogInformation("  Authorization: {Auth}", beforeAuth != null ? $"{beforeAuth.Scheme} {beforeAuth.Parameter?[..10]}..." : "None");

                // Existing logic...
                // ⚙️ Luôn dùng system token cho internal API calls
                _logger.LogInformation("🔑 Requesting system token from AuthService...");

                var systemTokenResponse = await _authApiClient.GetSystemToken(Guid.Empty, "system");
                if (systemTokenResponse.Success && !string.IsNullOrEmpty(systemTokenResponse.Data))
                {
                    token = systemTokenResponse.Data;
                    _logger.LogInformation("✅ Successfully retrieved system token");
                }
                else
                {
                    _logger.LogError("❌ Failed to retrieve system token from AuthService");
                }



                // Log final state
                //_logger.LogInformation("🔍 === HTTPCLIENT HEADERS (AFTER) ===");
                //var afterAuth = request.Headers.Authorization;
                //_logger.LogInformation("  Authorization: {Auth}", afterAuth != null ? $"{afterAuth.Scheme} {afterAuth.Parameter?[..10]}..." : "None");
                //_logger.LogInformation("🔍 === END HEADER HANDLER DEBUG ===");
            }

            // 👉 Gán vào Authorization header
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
