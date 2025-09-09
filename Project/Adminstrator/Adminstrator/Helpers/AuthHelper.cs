using System.IdentityModel.Tokens.Jwt;

namespace Adminstrator.Helpers
{
    public static class AuthHelper
    {
        public static (bool isAuth, string userId) GetUserInfo(HttpContext context)
        {

            var token = context.Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token))
                return (false, "");

            if (IsTokenValid(token))
            {
                try
                {
                    var jwtHandler = new JwtSecurityTokenHandler();
                    var jwtToken = jwtHandler.ReadJwtToken(token);

                    if (jwtToken.ValidTo <= DateTime.UtcNow)
                        return (false, "");

                    var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? "";

                    context.Session.SetString("UserId", userId);

                    return (true, userId);
                }
                catch
                {
                    return (false, "");
                }
            }

            return (false, "");

        }

        private static bool IsTokenValid(string token)
        {
            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(token);
                return jwtToken.ValidTo > DateTime.UtcNow;
            }
            catch { return false; }
        }
    }
}
