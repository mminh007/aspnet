
namespace Frontend.Helpers
{
    public class SettingsHelper
    {
        private static IConfiguration? _configuration;

        public static void Configure(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string GetAdminUrl(string key)
        {
            return _configuration?[$"AdminUrls:{key}"] ?? string.Empty;
        }
    }
}
