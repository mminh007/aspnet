using Backend.User.Models.Requests;
using static Backend.User.Models.DTOs;

namespace Backend.User.Repository
{
    public interface IUserRepository
    {
        Task<Guid?> CreateUserAsync(UserCheckModel user);     
        Task<int> DeleteUserAsync(Guid userId);             
        Task<UserDto?> GetUserByIdAsync(Guid userId);        
        Task<UserDto?> UpdateUserAsync(UserUpdateModel model);

    }
}
