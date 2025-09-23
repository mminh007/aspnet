using Frontend.Models.Auth;

namespace Frontend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string AccessToken, string RefreshToken,
           int ExpiresIn, string Role, string UserId, string Message, int StatusCode)> Login(LoginModel model);
        Task<(bool Success, string newAccessToken, string newRefreshToken, int expiresIn, string Role, string Message, int statusCode)> RefreshToken(string refreshToken);
        Task<(bool Success, string Message, int StatusCode)> Register(RegisterModel model);
    }
}

