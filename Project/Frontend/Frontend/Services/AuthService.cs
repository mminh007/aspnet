using Frontend.Helpers;
using Frontend.HttpsClients.Auths;
using Frontend.Models.Auth;
using Frontend.Services.Interfaces;
using System.Net.WebSockets;

namespace Frontend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthApiClient _authApi;

        public AuthService(IAuthApiClient authApi)
        {
            _authApi = authApi;
        }

        public async Task<(bool Success, string AccessToken, string RefreshToken, int ExpiresIn, string Role, string UserId, string Message, int StatusCode)> Login(LoginModel model)
        {
            model.ClientType = "frontend";

            var (success, accessToken, refreshToken, expiresIn, message, statusCode, role)
                = await _authApi.LoginAsync(model);

            if (!success || string.IsNullOrEmpty(accessToken))
                return (false, "", "", 0, "", "", message ?? "Login failed", statusCode);

            // Parse token để lấy userId
            var (isAuth, userId, Email, _) = AuthHelper.ParseUserIdFromToken(accessToken);

            return (isAuth, accessToken, refreshToken, expiresIn, role, userId, message ?? "Login successful", statusCode);
        }

        public async Task<(bool Success, string newAccessToken, string newRefreshToken, int expiresIn, string Role, string Message, int statusCode)> RefreshToken(string refreshToken)
        {
            var (Success, newAccessToken, newRefreshToken, expiresIn, Message, StatusCode, Role) = await _authApi.RefreshTokenAsync(refreshToken);

            return (Success, newAccessToken, newRefreshToken, expiresIn, Role, Message, StatusCode);
        }

        public async Task<(bool Success, string Message, int StatusCode)> Register(RegisterModel model)
        {
            var (success, message, statusCode) = await _authApi.RegisterAsync(model);
            return (success, message ?? (success ? "Register successful" : "Register failed"), statusCode);
        }
    }
}
