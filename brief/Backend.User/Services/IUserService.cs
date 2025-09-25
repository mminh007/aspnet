using Backend.User.Models.Requests;
using Backend.User.Models.Responses;
using static Backend.User.Models.DTOs;

namespace Backend.User.Services
{
    public interface IUserService
    {
        Task<UserApiResponse<Guid>> RegisterAsync(UserCheckModel model);
        Task<UserApiResponse<UserDto>> GetUserAsync(Guid userId);
        Task<UserApiResponse<UserDto>> UpdateUserAsync(UserUpdateModel model, Guid userId);
        Task<UserApiResponse<Guid>> DeleteUserAsync(Guid userId);
    }
}
