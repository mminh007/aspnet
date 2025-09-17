namespace Frontend.Helpers
{
    public class HeaderHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HeaderHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
