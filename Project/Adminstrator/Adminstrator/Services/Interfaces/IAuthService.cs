using Adminstrator.Models.Auths.Requests;

namespace Adminstrator.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string AccessToken, string RefreshToken,
           int ExpiresIn, string Role, string UserId, string Message, int StatusCode, bool? verifyEmail)> Login(LoginModel model);

        Task<(bool Success, string Message, int StatusCode)> Register(RegisterModel model);

        Task<(bool Success, string newAccessToken, string newRefreshToken, int expiresIn, string Role, string Message, int statusCode)> RefreshToken(string refreshToken);

        Task<(bool Success, string Message, int StatusCode, string Data)> VerifyEmail(VerifyEmailRequest model);

        Task<(bool Success, string Message, int StatusCode, string Data)> ResendCode(ResendCodeRequest model);

        Task<(bool Success, string Message, int StatusCode, string Data)> ForgotPassword(string email);

        Task<(bool Success, string Message, int StatusCode, string Data)> ResetPassword(ResetPasswordRequestModel model);


    }
}
