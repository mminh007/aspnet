using Backend.Authentication.Enums;
using Backend.Authentication.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace Backend.Authentication.Services
{
    public interface ITokenService
    {

        Task<OperationResult> RegisterAsync(RegisterRequestModel request, Guid UserId);

        Task<LoginResponseModel> Authenticate(LoginRequestModel model);

        Task<LoginResponseModel> GenerateTokenAsync(IdentityModel user);
        Task<RefreshTokenResponseModel> RefreshTokenAsync(IdentityModel user, int ResfreshTokenDays);

        Task<LoginResponseModel> ValidateRefreshTokenAsync(string rawToken);
    }
}
