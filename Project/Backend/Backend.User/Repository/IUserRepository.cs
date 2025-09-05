using Backend.User.Enums;
using Backend.User.Models;

namespace Backend.User.Repository
{
    public interface IUserRepository
    {
        Task<(bool Success, string? ErrorMessage, UserUpdateModel? UserInfo)> UpdateUserAsync(UserUpdateModel model);

        Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(Guid UserId);

        Task<(bool Success, Guid UserId, string? ErrorMessage)> CreateUserAsync(UserCheckModel user);

        Task<(bool Success, string? ErrorMessage, UserUpdateModel? UserInfo)> GetUserByIdAsync(Guid UserId);

    }
}
