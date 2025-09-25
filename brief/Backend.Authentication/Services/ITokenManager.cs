using Backend.Authentication.Models.Responses;
using static Backend.Authentication.Models.DTOs;

namespace Backend.Authentication.Services
{
    public interface ITokenManager
    {
        Task<AuthResponseModel<TokenDto>> GenerateTokenAsync(IdentityDto identity);
        Task<AuthResponseModel<TokenDto>> ValidateRefreshTokenAsync(string rawToken);
        string GenerateInternalServiceToken(string serviceName = "InternalService");
    }
}
