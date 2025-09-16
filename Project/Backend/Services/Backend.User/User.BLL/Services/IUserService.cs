using User.Common.Models.Requests;
using User.Common.Models.Responses;
using static User.Common.Models.DTOs;


namespace User.BLL.Services
{
    public interface IUserService
    {
        Task<UserApiResponse<List<Guid>>> GetStoreIdByUserId(Guid userId);
        Task<UserApiResponse<Guid?>> RegisterAsync(UserCheckModel model);
        Task<UserApiResponse<UserDto>> GetUserAsync(Guid userId);
        Task<UserApiResponse<UserDto>> UpdateUserAsync(UserUpdateModel model, Guid userId);
        Task<UserApiResponse<Guid>> DeleteUserAsync(Guid userId);
    }
}
