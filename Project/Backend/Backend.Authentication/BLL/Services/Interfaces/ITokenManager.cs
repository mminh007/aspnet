using Commons.Models.Responses;
using static Commons.Models.DTOs;

namespace BLL.Services.Interfaces
{
    public interface ITokenManager
    {
        Task<AuthResponseModel<TokenDto>> GenerateTokenAsync(IdentityDto identity);
        Task<AuthResponseModel<TokenDto>> ValidateRefreshTokenAsync(string rawToken);
        string GenerateInternalServiceToken(string serviceName = "InternalService");
    }
}
