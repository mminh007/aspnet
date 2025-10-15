using Auth.Common.Models.Requests;
using Auth.Common.Models.Responses;
using System.Threading.Tasks;
using static Auth.Common.Models.DTOs;

namespace Auth.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseModel<string>> RegisterAsync(RegisterRequestModel request, Guid userId);
        Task<AuthResponseModel<TokenDto>> Authenticate(LoginRequestModel request);
        Task<AuthResponseModel<string>> RegisterWithUserApiAsync(RegisterRequestModel request);

        AuthResponseModel<string> GetTokenSystemAsync();

        Task<AuthResponseModel<string>> VerifyEmailAsync(string email, string code);
        Task<AuthResponseModel<string>> ResendVerificationCodeAsync(string email);

        Task<AuthResponseModel<string>> ResetPasswordAsync(string email, string token, string newPassword);
        Task<AuthResponseModel<string>> SendForgotPasswordEmailAsync(string email);
    }
}
