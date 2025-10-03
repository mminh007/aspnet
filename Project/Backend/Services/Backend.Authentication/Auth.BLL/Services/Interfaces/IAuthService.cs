using Auth.Common.Models.Requests;
using Auth.Common.Models.Responses;
using static Auth.Common.Models.DTOs;

namespace Auth.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseModel<string>> RegisterAsync(RegisterRequestModel request, Guid userId);
        Task<AuthResponseModel<TokenDto>> Authenticate(LoginRequestModel request);
        Task<AuthResponseModel<string>> RegisterWithUserApiAsync(RegisterRequestModel request);

        AuthResponseModel<string> GetTokenSystemAsync();
    }
}
