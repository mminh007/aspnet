using Backend.User.Enums;
using Backend.User.Models;

namespace Backend.User.Repository
{
    public interface IUserRepository
    {   
        Task<UserResponseModel> UpdateUserAsync(UpdateUser model);

        Task<UserResponseModel> DeleteUserAsync(Guid UserId);

        Task<UserResponseModel> CreateUserAsync(UserCheckModel user);

    }
}
