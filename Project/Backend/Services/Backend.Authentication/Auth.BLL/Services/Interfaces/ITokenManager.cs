using Auth.Common.Models.Responses;
using static Auth.Common.Models.DTOs;

namespace Auth.BLL.Services.Interfaces
{
    public interface ITokenManager
    {
        Task<AuthResponseModel<TokenDto>> GenerateTokenAsync(IdentityDto identity);
        Task<AuthResponseModel<TokenDto>> ValidateRefreshTokenAsync(string rawToken);
        string GenerateInternalServiceToken(string serviceName = "InternalService");
    }
}
