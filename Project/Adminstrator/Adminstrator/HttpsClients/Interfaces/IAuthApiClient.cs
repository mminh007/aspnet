using Adminstrator.Models.Auths.Requests;

namespace Adminstrator.HttpsClients.Interfaces
{
    public interface IAuthApiClient
    {
        Task<(bool Success, string? AccessToken, string? RefreshToken, int ExpiresIn, string? Message, int statusCode, string Role)> LoginAsync(LoginModel model);
        Task<(bool Success, string? Message, int statusCode)> RegisterAsync(RegisterModel model);

        Task<(bool Success, string? AccessToken, string? RefreshToken, int ExpiresIn,
            string? Message, int statusCode, string Role)> RefreshTokenAsync(string refreshToken);
    }
}
