using Commons.Models.Requests;
using Commons.Models.Responses;
using static Commons.Models.DTOs;

namespace BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseModel<string>> RegisterAsync(RegisterRequestModel request, Guid userId);
        Task<AuthResponseModel<TokenDto>> Authenticate(LoginRequestModel request);
        Task<AuthResponseModel<string>> RegisterWithUserApiAsync(RegisterRequestModel request);
    }
}
