using Backend.Authentication.Models.Requests;
using Backend.Authentication.Models.Responses;
using static Backend.Authentication.Models.DTOs;

namespace Backend.Authentication.Services
{
    public interface IAuthService
    {
        Task<AuthResponseModel<string>> RegisterAsync(RegisterRequestModel request, Guid userId);
        Task<AuthResponseModel<TokenDto>> Authenticate(LoginRequestModel request);
        Task<AuthResponseModel<string>> RegisterWithUserApiAsync(RegisterRequestModel request);
    }
}
