using User.Common.Models.Requests;
using static User.Common.Models.DTOs;

namespace User.DAL.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task<List<Guid>> GetStoreIdsByUserIdAsync(Guid userId);
        Task<Guid> CheckUserAsync(UserCheckModel user);
        Task<Guid?> CreateUserAsync(UserCheckModel user, Guid? storeId);
        Task<int> DeleteUserAsync(Guid userId);
        Task<UserDto?> GetUserByIdAsync(Guid userId);
        Task<UserDto?> UpdateUserAsync(UserUpdateModel model);

    }
}
