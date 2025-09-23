namespace Frontend.Helpers
{
    public class HeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<HeaderHandler> _logger;

        public HeaderHandler(IHttpContextAccessor httpContextAccessor, ILogger<HeaderHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // Thêm vào HeaderHandler để debug chi tiết hơn
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

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
                var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
                var accessToken = httpContext.Request.Cookies["accessToken"];

                if (!string.IsNullOrWhiteSpace(authHeader))
                {
                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = authHeader.Substring(7);
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                        _logger.LogInformation("✅ Added Authorization header from request header");
                    }
                }
                else if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    _logger.LogInformation("✅ Added Authorization header from cookie");
                }
                else
                {
                    _logger.LogWarning("⚠️ No token found in either header or cookie");
                }

                // Log final state
                //_logger.LogInformation("🔍 === HTTPCLIENT HEADERS (AFTER) ===");
                //var afterAuth = request.Headers.Authorization;
                //_logger.LogInformation("  Authorization: {Auth}", afterAuth != null ? $"{afterAuth.Scheme} {afterAuth.Parameter?[..10]}..." : "None");
                //_logger.LogInformation("🔍 === END HEADER HANDLER DEBUG ===");
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
