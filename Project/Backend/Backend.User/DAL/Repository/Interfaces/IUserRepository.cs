using Commons.Models.Requests;
using static Commons.Models.DTOs;

namespace DAL.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<Guid?> CreateUserAsync(UserCheckModel user);
        Task<int> DeleteUserAsync(Guid userId);
        Task<UserDto?> GetUserByIdAsync(Guid userId);
        Task<UserDto?> UpdateUserAsync(UserUpdateModel model);

    }
}
