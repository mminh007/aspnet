using Commons.Models.Requests;
using Commons.Models.Responses;
using static Commons.Models.DTOs;

namespace BLL.Services
{
    public interface IUserService
    {
        Task<UserApiResponse<Guid>> RegisterAsync(UserCheckModel model);
        Task<UserApiResponse<UserDto>> GetUserAsync(Guid userId);
        Task<UserApiResponse<UserDto>> UpdateUserAsync(UserUpdateModel model, Guid userId);
        Task<UserApiResponse<Guid>> DeleteUserAsync(Guid userId);
    }
}
