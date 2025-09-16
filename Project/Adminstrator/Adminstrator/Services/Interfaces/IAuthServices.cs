using Adminstrator.Models.Auths.Requests;

namespace Adminstrator.Services.Interfaces
{
    public interface IAuthServices
    {
        Task<(bool Success, string AccessToken, string RefreshToken, 
            int ExpiresIn, string Role, string UserId, string Message, int StatusCode)> Login(LoginModel model);

        Task<(bool Success, string Message, int StatusCode)> Register(RegisterModel model);
    }
}
